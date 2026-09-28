using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.Networx;

public interface INetworxAuctionTaskLookup
{
    Task<List<string>> FindAsync(int leadTypeId, string option, CancellationToken ct);
}
public sealed class SqlNetworxAuctionTaskLookup(MyApp.Api.Data.MyAppDbContext db) : INetworxAuctionTaskLookup
{
    public Task<List<string>> FindAsync(int leadTypeId, string option, CancellationToken ct) =>
        db.ExternalPlatformTaskMappings.AsNoTracking().Where(x => x.PlatformCode == "NETWORX" &&
            x.LeadTypeId == leadTypeId && x.InternalOptionCode == option && x.IsActive)
            .Select(x => x.ExternalTaskId).ToListAsync(ct);
}
public partial class NetworxLeadProvider : IAuctionProvider
{
    private readonly INetworxAuctionTaskLookup _auctionTasks;
    public static readonly IReadOnlyDictionary<string, string[]> RequiredTaskOptions = new Dictionary<string, string[]> {
        ["Roofing"] = ["ROOF_REPLACE_ASPHALT"],
        ["Windows"] = ["WINDOW_GLASS_INSTALL_REPLACE", "WINDOW_FRAME_GLASS_REPAIR", "WINDOW_INSTALL_MULTIPLE", "WINDOW_INSTALL_SINGLE"],
        ["HVAC"] = ["HVAC_CENTRAL_AC_INSTALL", "HVAC_CENTRAL_AC_REPAIR", "HVAC_HEAT_PUMP_INSTALL", "HVAC_HEAT_PUMP_REPAIR", "HVAC_FURNACE_INSTALL", "HVAC_FURNACE_REPAIR"],
        ["Bathroom"] = ["BATH_WALK_IN_TUB", "BATH_TUB_TO_SHOWER", "BATH_TUB_SHOWER_INSTALL", "BATHROOM_REMODEL"] };
    public static string? AuctionTaskOption(AuctionFacts facts) => ResolveInternalOptionCode(facts.Vertical, facts.Qualifications);
    public bool IsEnabled(string vertical) => RequiredTaskOptions.ContainsKey(vertical) && _options.Enabled && _options.UsePingPost &&
        Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var endpoint) && endpoint.Scheme == "https" &&
        !string.IsNullOrWhiteSpace(_options.AccessKey) && !string.IsNullOrWhiteSpace(_options.UserId);
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
    {
        var lead = facts.ToMappingLead();
        var option = AuctionTaskOption(facts);
        if (option == null || facts.LeadTypeId == null)
            return new(false, null, null, null, "{}", "{}", "Unsupported Networx project/vertical; no task mapping can be selected.");
        var tasks = await _auctionTasks.FindAsync(facts.LeadTypeId.Value, option, ct);
        if (tasks.Count != 1 || string.IsNullOrWhiteSpace(tasks[0]))
            return new(false, null, null, null, "{}", "{}", $"Networx requires exactly one active task mapping: LeadTypeId={facts.LeadTypeId}, option={option}.");
        var task = tasks[0];
        var payload = BuildPingFields(lead, task, NormalizeSourceId(_options.SourceId));
        using var response = await SendAsync(payload, ct);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && IsSuccessful(parsed);
        var price = AuctionPolicy.Price(parsed.Price);
        return new(accepted, price, parsed.Token, null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, price, (int)response.StatusCode), accepted ? null : "Provider rejected ping.", task);
    }
    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
    {
        var validation = ValidateLead(lead);
        if (validation != null || string.IsNullOrWhiteSpace(context))
            return new(false, false, false, null, "{}", "{}", validation ?? "Task snapshot missing.");
        var payload = BuildPostFields(lead, context, NormalizeSourceId(_options.SourceId), reference);
        using var response = await SendAsync(payload, ct);
        var parsed = ParseResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && IsSuccessful(parsed);
        return new(accepted, false, !accepted && (int)response.StatusCode >= 500, parsed.SuccessCode ?? reference,
            AuctionAudit.Payload(payload), AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : "Provider rejected post or returned an unconfirmed outcome.", (int)response.StatusCode);
    }
}
