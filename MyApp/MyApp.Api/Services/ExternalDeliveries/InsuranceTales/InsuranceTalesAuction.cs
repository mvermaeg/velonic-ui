using System.Globalization;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.InsuranceTales;

public sealed partial class InsuranceTalesLeadProvider : IAuctionProvider
{
    public bool IsEnabled(string vertical) => vertical == "Windows" && _options.Enabled && _options.Windows.Enabled &&
        !string.IsNullOrWhiteSpace(_options.Windows.CampaignId) && !string.IsNullOrWhiteSpace(_options.Windows.CampaignKey) &&
        AuctionPolicy.HttpsEndpoint(_options.PingUrl) && AuctionPolicy.HttpsEndpoint(_options.PostUrl);
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
    {
        var count = GetWindowsCount(facts.ToMappingLead());
        var payload = BuildPingFields(facts.ToMappingLead(), count);
        if (facts.IsTest) payload["lp_test"] = "1";
        using var content = new FormUrlEncodedContent(payload);
        using var response = await _httpClient.PostAsync(_options.PingUrl, content, ct);
        var parsed = ParsePingResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && parsed.Success;
        return new(accepted, parsed.Price, parsed.PingId, null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, parsed.Price, (int)response.StatusCode), accepted ? null : "Provider rejected ping.", count.ToString(CultureInfo.InvariantCulture));
    }
    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
    {
        var validation = ValidateLeadForInsuranceTales(lead);
        if (validation != null || !int.TryParse(context, out var count))
            return new(false, false, false, null, "{}", "{}", validation ?? "Window-count snapshot missing.");
        var payload = BuildPostFields(lead, reference, count);
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
