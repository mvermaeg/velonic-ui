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
        => await PingWithComplianceAsync(facts, null, null, false, ct);

    public async Task<AuctionOffer> PingWithComplianceAsync(AuctionFacts facts, string? certificate, string? jornaya,
        bool requireProof, CancellationToken ct)
    {
        var validCertificate = Uri.TryCreate(certificate, UriKind.Absolute, out var cert) &&
            cert.Scheme == "https" && cert.Host.Equals("cert.trustedform.com", StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrEmpty(cert.UserInfo) && cert.AbsolutePath.Length > 1;
        var validJornaya = Guid.TryParse(jornaya, out _);
        if (requireProof && !validCertificate && !validJornaya)
            return new(false, null, null, null, "{}", "{}", "BLUEINK requires valid TrustedForm or Jornaya proof on ping; no ping sent.");
        var settings = _options.Verticals[facts.Vertical];
        var payload = BuildPayload(facts.ToMappingLead(), facts.Vertical, null, false);
        if (facts.Vertical == "Roofing" && !HasRoofingMapping(payload))
            return new(false, null, null, null, "{}", "{}", "BLUEINK Roofing requires approved project and material mappings; no ping sent.");
        // Existing meta mixes post compliance with ping. No undocumented certificates in auction pings.
        if (payload["meta"] is Dictionary<string, object?> meta)
        {
            meta.Remove("trusted_form_cert_url");
            meta.Remove("lead_id_code");
            if (validCertificate) meta["trusted_form_cert_url"] = certificate;
            if (validJornaya) meta["lead_id_code"] = jornaya;
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
        if (vertical == "Roofing" && !HasRoofingMapping(payload))
            return new(false, false, false, null, "{}", "{}", "BLUEINK Roofing mapping unavailable; no post sent.");
        using var response = await SendAsync(_options.UseTestEndpoints || lead.IsTest ? settings.TestPostUrl : settings.ProductionPostUrl,
            settings.Token, JsonSerializer.Serialize(payload, JsonOptions), ct);
        var result = ParseResponse(await response.Content.ReadAsStringAsync(ct));
        var accepted = response.IsSuccessStatusCode && result.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
        return new(accepted, false, !accepted && (int)response.StatusCode >= 500, result.ConfirmationId ?? reference,
            AuctionAudit.Payload(payload), AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : "Provider rejected post or returned an unconfirmed outcome.", (int)response.StatusCode);
    }

    private static string? MapRoofingValue(Dictionary<string, string> approved, string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var mapped = approved.FirstOrDefault(x => string.Equals(x.Key.Trim(), input.Trim(), StringComparison.OrdinalIgnoreCase)).Value;
        // Already canonical input is accepted only if it appears in the approved values.
        mapped ??= approved.Values.FirstOrDefault(x => string.Equals(x, input.Trim(), StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrWhiteSpace(mapped) ? null : mapped;
    }

    private string? MapRoofingProject(Dictionary<string, JsonElement> answers)
    {
        var project = GetString(answers, "projectType") ?? GetString(answers, "roofingProject");
        if (string.Equals(project?.Trim(), "Install New Roof", StringComparison.OrdinalIgnoreCase))
            return GetString(answers, "roofInstallationHomeType") switch
            {
                "Newly constructed home" => "New roof for new home",
                "Existing home" => "New roof for an existing home",
                _ => null
            };
        return MapRoofingValue(_options.RoofingProjectTypes, project);
    }

    private static bool HasRoofingMapping(Dictionary<string, object?> payload) =>
        payload["data"] is Dictionary<string, object?> data && data.TryGetValue("roof", out var value) &&
        value is Dictionary<string, object?> roof && roof.Count == 2 &&
        roof.Values.All(x => x is string s && !string.IsNullOrWhiteSpace(s));
}
