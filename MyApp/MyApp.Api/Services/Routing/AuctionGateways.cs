using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using MyApp.Api.Services.ExternalDeliveries;
using MyApp.Api.Data;
using Microsoft.EntityFrameworkCore;
using TT = MyApp.Api.Services.ExternalDeliveries.Thumbtack;

namespace MyApp.Api.Services.Routing;

public sealed record ThumbtackCheck(bool Eligible, bool Available, string Reason, string? SearchId = null, string? Response = null);
public interface IThumbtackAuctionGateway
{
    Task<bool> IsCoveredAsync(AuctionWork work, CancellationToken ct);
    Task<ThumbtackCheck> CheckAsync(AuctionWork work, CancellationToken ct);
}
public sealed class ThumbtackAuctionGateway(TT.ThumbtackEligibilityService eligibility, TT.ThumbtackApiService api,
    IOptions<TT.ThumbtackOptions> options, MyAppDbContext db) : IThumbtackAuctionGateway
{
    public Task<bool> IsCoveredAsync(AuctionWork w, CancellationToken ct) =>
        Thumbtack.ThumbtackCoverageLookup.HasCoverageAsync(db, w.Run.VerticalCode, w.Lead.Postcode, ct);

    public async Task<ThumbtackCheck> CheckAsync(AuctionWork w, CancellationToken ct)
    {
        // Reserve under a cross-process service lock before the remote search. Ambiguous
        // searches consume capacity conservatively and are never replayed after restart.
        await using var reservation = await LeadExecutionLock.AcquireResourceAsync(db,
            $"Homeyy.Thumbtack.Cap.{w.Run.VerticalCode}", ct);
        if (reservation == null) throw new InvalidOperationException("Thumbtack capacity check busy.");
        var check = await eligibility.CheckAsync(w.Lead, w.Run.VerticalCode, ct);
        if (!check.Allowed) return new(false, false, check.Reason ?? "Not covered.");
        var category = w.Run.VerticalCode switch { "Roofing" => options.Value.Roofing, "Windows" => options.Value.Windows,
            "Bathroom" => options.Value.Bathroom, "Gutters" => options.Value.Gutters, _ => null };
        if (string.IsNullOrWhiteSpace(category?.CategoryPk)) return new(false, false, "Missing category mapping.");
        w.Run.ThumbtackSearchStartedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var response = await api.SearchBusinessesAsync(category.CategoryPk,
            Thumbtack.ThumbtackCoverageLookup.NormalizeZip(w.Lead.Postcode)!, $"HOMEYY-AUCTION-{w.Run.Id}", 10, ct);
        return new(true, response.Data.Count > 0, response.Data.Count > 0 ? "Available" : "No live businesses",
            response.SearchId, System.Text.Json.JsonSerializer.Serialize(response));
    }
}
public sealed record ProviderCapability(string PlatformCode, bool TruePingPost, bool MonetaryBid, string PostReference,
    string Verticals, string Compliance, string Endpoints, string? Blocker);
public interface IAuctionGateway
{
    bool Enabled(string platform, string vertical);
    bool Idempotent(string platform);
    Task<AuctionOffer> PingAsync(string platform, AuctionFacts facts, CancellationToken ct);
    Task<AuctionPost> PostAsync(string platform, AuctionWork work, string reference, string? context, string key, CancellationToken ct);
    string Protect(string value);
    string Unprotect(string value);
}
public sealed class AuctionGateway(IServiceScopeFactory scopes, IDataProtectionProvider protection) : IAuctionGateway
{
    private readonly IDataProtector _protector = protection.CreateProtector("Homeyy.ExternalAuction.PostReferences.v1");
    public static readonly ProviderCapability[] Capabilities = [
        new("BLUEINK", true, true, "auth_code", "Configured verticals", "TrustedForm or Jornaya required on ping; contact data on winner post only", "Configured test/production ping and post", null),
        new("NETWORX", true, true, "token", "Active task mappings", "TrustedForm/consent on post", "Configured BaseUrl; test ZIP 00001", null),
        new("Mili", true, true, "ping_id", "Roofing, Windows, HVAC, Bathroom", "Existing post compliance", "Configured ping/post; lp_test", null),
        new("InsuranceTales", true, true, "ping_id", "Windows", "Existing post compliance", "Configured ping/post; lp_test", null),
        new("Modernize", true, true, "pingToken", "Roofing, Windows, Bathroom, HVAC", "TrustedForm required on winner post", "Configured production ping/post", "Requires production tagId and provider approval") ];
    private static IAuctionProvider? Find(IServiceProvider services, string code) => services.GetServices<IExternalLeadProvider>()
        .OfType<IAuctionProvider>().SingleOrDefault(x => string.Equals(x.PlatformCode, code, StringComparison.OrdinalIgnoreCase));
    public bool Enabled(string platform, string vertical) { if (AuctionProviderPolicy.Excluded(platform)) return false; using var s = scopes.CreateScope(); return Find(s.ServiceProvider, platform)?.IsEnabled(vertical) == true; }
    public bool Idempotent(string platform) { using var s = scopes.CreateScope(); return Find(s.ServiceProvider, platform)?.SupportsIdempotentPost == true; }
    public async Task<AuctionOffer> PingAsync(string platform, AuctionFacts facts, CancellationToken ct)
    {
        if (AuctionProviderPolicy.Excluded(platform)) throw new InvalidOperationException("Provider excluded from auction.");
        using var s = scopes.CreateScope();
        var provider = Find(s.ServiceProvider, platform);
        if (provider == null || !provider.IsEnabled(facts.Vertical))
            throw new InvalidOperationException("Provider disabled or unavailable.");
        if (platform.Equals("BLUEINK", StringComparison.OrdinalIgnoreCase))
        {
            var db = s.ServiceProvider.GetRequiredService<MyAppDbContext>();
            var proof = await db.Leads.AsNoTracking().Where(l => l.Id == facts.LeadId)
                .Select(l => new { l.TrustedFormCertificateUrl, l.JornayaLeadId }).SingleAsync(ct);
            return await ((ExternalDeliveries.BlueInk.BlueInkLeadProvider)provider)
                .PingWithComplianceAsync(facts, proof.TrustedFormCertificateUrl, proof.JornayaLeadId, true, ct);
        }
        if (platform == "Mili")
        {
            var db = s.ServiceProvider.GetRequiredService<MyAppDbContext>();
            while (true)
            {
                TimeSpan wait;
                await using (var cap = await LeadExecutionLock.AcquireResourceAsync(db, $"Homeyy.Mili.PingQuota.{facts.Vertical}", ct))
                {
                    if (cap == null) wait = TimeSpan.FromMilliseconds(250);
                    else
                    {
                        var now = DateTime.UtcNow;
                        var history = db.LeadRoutingAttempts.Where(a => a.PlatformCode == "Mili" && a.LeadRoutingRun.VerticalCode == facts.Vertical);
                        var times = await history.Where(a => a.PingDispatchedOn >= now.AddDays(-1))
                            .Select(a => a.PingDispatchedOn!.Value).ToListAsync(ct);
                        wait = MiliQuotaPolicy.Wait(times, now);
                        if (wait <= TimeSpan.Zero)
                        {
                            var attempt = await history.SingleAsync(a => a.LeadId == facts.LeadId && a.Status == "PingStarted", ct);
                            if (attempt.PingDispatchedOn != null)
                                return new(false, null, null, null, "{}", "{}", "Mili ping already reserved; no replay.");
                            attempt.PingDispatchedOn = now;
                            await db.SaveChangesAsync(ct);
                            break;
                        }
                    }
                }
                // Wait without holding the database lock. The auction cancellation token
                // prevents sending after its deadline; restart never replays a reserved ping.
                await Task.Delay(wait, ct);
            }
            var proof = await db.Leads.AsNoTracking().Where(l => l.Id == facts.LeadId)
                .Select(l => new ExternalDeliveries.Mili.MiliPingCompliance(l.JornayaLeadId,
                    l.TrustedFormCertificateUrl, l.IsTcpaCompliant, l.TcpaComplianceText, l.UserAgent)).SingleAsync(ct);
            return await ((ExternalDeliveries.Mili.MiliLeadProvider)provider)
                .PingWithComplianceAsync(facts, proof, false, ct);
        }
        return await Find(s.ServiceProvider, platform)!.PingAsync(facts, ct);
    }
    public async Task<AuctionPost> PostAsync(string platform, AuctionWork work, string reference, string? context, string key, CancellationToken ct)
    { if (AuctionProviderPolicy.Excluded(platform)) throw new InvalidOperationException("Provider excluded from auction."); using var s = scopes.CreateScope(); return await Find(s.ServiceProvider, platform)!.PostAsync(work.Lead, work.Run.VerticalCode, reference, context, key, ct); }
    public string Protect(string value) => _protector.Protect(value);
    public string Unprotect(string value) => _protector.Unprotect(value);
}
