using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.Mili;

public sealed partial class MiliLeadProvider : IAuctionProvider
{
    public bool IsEnabled(string vertical) => _options.Enabled && _options.ProductionApproved && !_options.UseTestMode && ResolveCampaign(vertical) is { Enabled: true } c &&
        !string.IsNullOrWhiteSpace(c.CampaignId) && !string.IsNullOrWhiteSpace(c.CampaignKey) &&
        !string.IsNullOrWhiteSpace(_options.PubId) && !string.IsNullOrWhiteSpace(_options.InternalSourceId) &&
        AuctionPolicy.HttpsEndpoint(_options.PingUrl) && AuctionPolicy.HttpsEndpoint(_options.PostUrl);
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
        => await PingAuctionAsync(facts, null, "Exclusive", ct);
    public Task<AuctionOffer> PingSharedAsync(AuctionFacts facts, CancellationToken ct)
        => PingAuctionAsync(facts, null, "Shared", ct);
    public Task<AuctionOffer> PingWithComplianceAsync(AuctionFacts facts, MiliPingCompliance compliance, bool shared, CancellationToken ct)
        => PingAuctionAsync(facts, compliance, shared ? "Shared" : "Exclusive", ct);
    private async Task<AuctionOffer> PingAuctionAsync(AuctionFacts facts, MiliPingCompliance? compliance, string type, CancellationToken ct)
    {
        var campaign = ResolveCampaign(facts.Vertical);
        if (!_options.Enabled || campaign is not { Enabled: true } || string.IsNullOrWhiteSpace(campaign.CampaignId) || string.IsNullOrWhiteSpace(campaign.CampaignKey))
            return new(false, null, null, null, "{}", "{}", "Mili campaign configuration missing; no ping sent.");
        if (!facts.IsTest && (!_options.ProductionApproved || _options.UseTestMode))
            return new(false, null, null, null, "{}", "{}", "Mili production blocked pending testing and buyer activation approval.");
        var mappingLead = facts.ToMappingLead();
        compliance?.Apply(mappingLead);
        var missing = ValidatePingCompliance(mappingLead);
        if (missing != null) return new(false, null, null, null, "{}", "{}", missing);
        if (!IsWithinMiliSchedule(out _))
            return new(false, null, null, null, "{}", "{}", "Mili outside permitted Pacific posting hours.");
        if (!_rateLimiter.TryAcquire(facts.Vertical, out _))
            return new(false, null, null, null, "{}", "{}", "Mili local per-vertical ping quota reached.");
        var project = ResolveProject(mappingLead, facts.Vertical);
        var payload = BuildPingFields(mappingLead, campaign, project);
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
        if (!lead.IsTest && (!_options.ProductionApproved || _options.UseTestMode))
            return new(false, false, false, null, "{}", "{}", "Mili production blocked pending testing and buyer activation approval.");
        var validation = ValidateLead(lead) ?? ValidatePingCompliance(lead);
        if (validation != null || context == null) return new(false, false, false, null, "{}", "{}", validation ?? "Project snapshot missing.");
        var payload = BuildPostFields(lead, ResolveCampaign(vertical)!, reference, context);
        payload["type"] = type;
        payload["lp_response"] = "xml";
        if (lead.IsTest) payload["lp_test"] = "1"; else payload.Remove("lp_test");
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PostUrl, content, ct);
        var parsed = ParsePostResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        var unknown = !accepted && ((int)response.StatusCode >= 500 || string.IsNullOrWhiteSpace(parsed.Error) ||
            parsed.Error.Contains("parse", StringComparison.OrdinalIgnoreCase));
        return new(accepted, false, unknown, parsed.LeadId ?? reference,
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
