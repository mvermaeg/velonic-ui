using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.Mili;

public sealed partial class MiliLeadProvider : IAuctionProvider
{
    public bool IsEnabled(string vertical) => _options.Enabled && ResolveCampaign(vertical) is { Enabled: true } c &&
        !string.IsNullOrWhiteSpace(c.CampaignId) && !string.IsNullOrWhiteSpace(c.CampaignKey) &&
        AuctionPolicy.HttpsEndpoint(_options.PingUrl) && AuctionPolicy.HttpsEndpoint(_options.PostUrl);
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
        => await PingAuctionAsync(facts, "Exclusive", ct);
    public Task<AuctionOffer> PingSharedAsync(AuctionFacts facts, CancellationToken ct)
        => PingAuctionAsync(facts, "Shared", ct);
    private async Task<AuctionOffer> PingAuctionAsync(AuctionFacts facts, string type, CancellationToken ct)
    {
        if (!IsWithinMiliSchedule(out _))
            return new(false, null, null, null, "{}", "{}", "Mili outside permitted Pacific posting hours.");
        if (!_rateLimiter.TryAcquire(facts.Vertical, out _))
            return new(false, null, null, null, "{}", "{}", "Mili local per-vertical ping quota reached.");
        var project = ResolveProject(facts.ToMappingLead(), facts.Vertical);
        var payload = BuildPingFields(facts.ToMappingLead(), ResolveCampaign(facts.Vertical)!, project);
        // The auction posts to exactly one locked winner. Legacy Shared configuration
        // does not describe this contract and can fail the provider's TYPE filter.
        payload["type"] = type;
        payload["lp_response"] = "xml";
        if (facts.IsTest) payload["lp_test"] = "1"; else payload.Remove("lp_test");
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PingUrl, content, ct);
        var parsed = ParsePingResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        return new(accepted, parsed.Price, parsed.PingId, null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, parsed.Price, (int)response.StatusCode), accepted ? null : SafeMiliError(parsed.Error), project);
    }
    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
        => await PostAuctionAsync(lead, vertical, reference, context, "Exclusive", ct);
    public Task<AuctionPost> PostSharedAsync(Lead lead, string vertical, string reference, string? context, CancellationToken ct)
        => PostAuctionAsync(lead, vertical, reference, context, "Shared", ct);
    private async Task<AuctionPost> PostAuctionAsync(Lead lead, string vertical, string reference, string? context, string type, CancellationToken ct)
    {
        var validation = ValidateLead(lead);
        if (validation != null || context == null) return new(false, false, false, null, "{}", "{}", validation ?? "Project snapshot missing.");
        var payload = BuildPostFields(lead, ResolveCampaign(vertical)!, reference, context);
        payload["type"] = type;
        payload["lp_response"] = "xml";
        if (lead.IsTest) payload["lp_test"] = "1"; else payload.Remove("lp_test");
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PostUrl, content, ct);
        var parsed = ParsePostResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        return new(accepted, false, !accepted && (int)response.StatusCode >= 500, parsed.LeadId ?? reference,
            AuctionAudit.Payload(payload), AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : SafeMiliError(parsed.Error), (int)response.StatusCode);
    }
    // Never persist raw provider error text: it may echo credentials or contacts.
    internal static string SafeMiliError(string? error)
    {
        var value = error ?? "";
        if (value.Contains("Invalid Campaign", StringComparison.OrdinalIgnoreCase)) return "Mili authentication failed: Invalid Campaign ID or Key.";
        if (value.Contains("No Match", StringComparison.OrdinalIgnoreCase)) return "Mili rejected: No Match.";
        if (value.Contains("TYPE", StringComparison.OrdinalIgnoreCase) && value.Contains("filter", StringComparison.OrdinalIgnoreCase)) return "Mili rejected: TYPE filter mismatch.";
        if (value.Contains("filter", StringComparison.OrdinalIgnoreCase))
        {
            // Retain only approved field names, never echoed values or contacts.
            foreach (var field in new[] { "Project", "zip_code", "state", "ip_address", "lp_campaign_id" })
                if (System.Text.RegularExpressions.Regex.IsMatch(value, @"\b" + field + @"\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    return "Mili rejected: " + field + " eligibility filter.";
        }
        if (value.Contains("duplicate", StringComparison.OrdinalIgnoreCase)) return "Mili rejected: Duplicate.";
        if (value.Contains("parse", StringComparison.OrdinalIgnoreCase)) return "Mili response malformed; acceptance unconfirmed.";
        return "Mili: " + ProviderFailureReason.Classify(value) + ".";
    }
}
