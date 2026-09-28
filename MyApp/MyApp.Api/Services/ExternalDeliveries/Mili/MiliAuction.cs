using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.Mili;

public sealed partial class MiliLeadProvider : IAuctionProvider
{
    public bool IsEnabled(string vertical) => _options.Enabled && ResolveCampaign(vertical) is { Enabled: true } c &&
        !string.IsNullOrWhiteSpace(c.CampaignId) && !string.IsNullOrWhiteSpace(c.CampaignKey) &&
        AuctionPolicy.HttpsEndpoint(_options.PingUrl) && AuctionPolicy.HttpsEndpoint(_options.PostUrl);
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
    {
        if (!IsWithinMiliSchedule(out _) || !_rateLimiter.TryAcquire(facts.Vertical, out _))
            return new(false, null, null, null, "{}", "{}", "Mili schedule or rate limit excluded this ping.");
        var project = ResolveProject(facts.ToMappingLead(), facts.Vertical);
        var payload = BuildPingFields(facts.ToMappingLead(), ResolveCampaign(facts.Vertical)!, project);
        if (facts.IsTest) payload["lp_test"] = "1";
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PingUrl, content, ct);
        var parsed = ParsePingResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        return new(accepted, parsed.Price, parsed.PingId, null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, parsed.Price, (int)response.StatusCode), accepted ? null : "Provider rejected ping.", project);
    }
    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
    {
        var validation = ValidateLead(lead);
        if (validation != null || context == null) return new(false, false, false, null, "{}", "{}", validation ?? "Project snapshot missing.");
        var payload = BuildPostFields(lead, ResolveCampaign(vertical)!, reference, context);
        if (lead.IsTest) payload["lp_test"] = "1";
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PostUrl, content, ct);
        var parsed = ParsePostResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        return new(accepted, false, !accepted && (int)response.StatusCode >= 500, parsed.LeadId ?? reference,
            AuctionAudit.Payload(payload), AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : "Provider rejected post or returned an unconfirmed outcome.", (int)response.StatusCode);
    }
}
