using System.Text.Json;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
namespace MyApp.Api.Services.ExternalDeliveries.BlueInk;

public partial class BlueInkLeadProvider : IAuctionProvider
{
    public bool IsEnabled(string vertical) => vertical is "Roofing" or "Windows" or "Bathroom" or "HVAC" && _options.Enabled && _options.UsePingPost &&
        _options.Verticals.TryGetValue(vertical, out var v) && v.Enabled && !string.IsNullOrWhiteSpace(v.Token) &&
        AuctionPolicy.HttpsEndpoint(v.TestPingUrl) && AuctionPolicy.HttpsEndpoint(v.TestPostUrl) &&
        (_options.UseTestEndpoints || AuctionPolicy.HttpsEndpoint(v.ProductionPingUrl) && AuctionPolicy.HttpsEndpoint(v.ProductionPostUrl));
    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
    {
        var settings = _options.Verticals[facts.Vertical];
        var payload = BuildPayload(facts.ToMappingLead(), facts.Vertical, null, false);
        // Existing meta mixes post compliance with ping. No undocumented certificates in auction pings.
        if (payload["meta"] is Dictionary<string, object?> meta)
        {
            meta.Remove("trusted_form_cert_url");
            meta.Remove("lead_id_code");
        }
        using var response = await SendAsync(_options.UseTestEndpoints || facts.IsTest ? settings.TestPingUrl : settings.ProductionPingUrl,
            settings.Token, JsonSerializer.Serialize(payload, JsonOptions), ct);
        var result = ParseResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && result.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
        var price = AuctionPolicy.Price(result.Price);
        return new(accepted, price, result.AuthCode, null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, price, (int)response.StatusCode), accepted ? null : "Provider rejected ping.");
    }
    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
    {
        var validation = ValidateLead(lead);
        if (validation != null) return new(false, false, false, null, "{}", "{}", validation);
        var settings = _options.Verticals[vertical];
        var payload = BuildPayload(lead, vertical, reference, true);
        using var response = await SendAsync(_options.UseTestEndpoints || lead.IsTest ? settings.TestPostUrl : settings.ProductionPostUrl,
            settings.Token, JsonSerializer.Serialize(payload, JsonOptions), ct);
        var result = ParseResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && result.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
        return new(accepted, false, !accepted && (int)response.StatusCode >= 500, result.ConfirmationId ?? reference,
            AuctionAudit.Payload(payload), AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : "Provider rejected post or returned an unconfirmed outcome.", (int)response.StatusCode);
    }
}
