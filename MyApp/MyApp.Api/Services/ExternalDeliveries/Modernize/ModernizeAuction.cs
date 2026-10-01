using System.Net.Http.Json;
using System.Text.Json;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;

namespace MyApp.Api.Services.ExternalDeliveries.Modernize;

// Contract: https://apidoc.modernize.com/publishers/ping-post.html
// Separate ping and winner-only post; never call legacy DeliverAsync from an auction.
public sealed partial class ModernizeLeadProvider : IAuctionProvider
{
    private string AuctionEndpoint => (_options.UseStaging ? _options.StagingBaseUrl : _options.ProductionBaseUrl).TrimEnd('/');
    private string AuctionTag => _options.UseStaging ? _options.StagingTagId : _options.ProductionTagId;
    public bool IsEnabled(string vertical) => vertical is "Roofing" or "Windows" or "Bathroom" or "HVAC" &&
        _options.Enabled && AuctionPolicy.HttpsEndpoint(AuctionEndpoint) &&
        !string.IsNullOrWhiteSpace(AuctionTag) && !string.IsNullOrWhiteSpace(_options.PartnerSourceId);

    private sealed record PingSnapshot(string Endpoint, Dictionary<string, JsonElement> Payload);

    public async Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct)
    {
        if (!IsEnabled(facts.Vertical)) return RejectPing("Modernize is disabled or not configured for this service.");
        var payload = AuctionPayload(facts);
        if (payload == null) return RejectPing("Modernize requires supported project/material answers and home ownership.");
        var endpoint = AuctionEndpoint;
        payload["tagId"] = AuctionTag;
        payload["partnerSourceId"] = _options.PartnerSourceId;
        var started = DateTime.UtcNow;
        using var response = await _httpClient.PostAsJsonAsync(endpoint + "/ping-post/pings", payload, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        try
        {
            using var doc = JsonDocument.Parse(body);
            var status = Field(doc.RootElement, "status");
            var token = Field(doc.RootElement, "pingToken");
            var amount = AuctionPolicy.Price(Field(doc.RootElement, "price"));
            var accepted = response.IsSuccessStatusCode && status == "success";
            // The documented token validity is up to 30 minutes; count from send time.
            return new(accepted, amount, token, started.AddMinutes(30), AuctionAudit.Payload(payload),
                AuctionAudit.Response(accepted, amount, (int)response.StatusCode),
                accepted ? null : "Modernize rejected ping or returned an API error.",
                JsonSerializer.Serialize(new { Endpoint = endpoint, Payload = payload }));
        }
        catch (JsonException) { return RejectPing("Modernize returned malformed ping JSON."); }
    }

    public async Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct)
    {
        var error = ValidateLead(lead);
        if (error != null) return RejectPost(error);
        if (!IsEnabled(vertical) || string.IsNullOrWhiteSpace(reference)) return RejectPost("Modernize winner is no longer enabled or has no saved ping token.");
        PingSnapshot? snapshot;
        try { snapshot = JsonSerializer.Deserialize<PingSnapshot>(context ?? "null"); }
        catch (JsonException) { return RejectPost("Modernize ping snapshot is invalid."); }
        if (snapshot?.Payload == null || snapshot.Endpoint != AuctionEndpoint ||
            !snapshot.Payload.TryGetValue("publisherSubId", out var id) || id.GetString() != $"HOMEYY-{lead.Id}")
            return RejectPost("Modernize saved ping does not match this lead or environment.");
        var state = UsState(lead.State);
        if (state == null) return RejectPost("Modernize requires a recognized US state.");
        var (first, last) = SplitName(lead.FullName);
        if (string.IsNullOrWhiteSpace(last)) return RejectPost("Modernize requires first and last names.");
        var payload = snapshot.Payload.ToDictionary(x => x.Key, x => (object?)x.Value);
        payload["pingToken"] = reference;
        payload["firstName"] = first; payload["lastName"] = last;
        payload["address"] = lead.Address; payload["city"] = lead.City; payload["state"] = state;
        payload["phone"] = NormalizePhone(lead.Phone); payload["email"] = lead.Email;
        payload["homePhoneConsentLanguage"] = lead.TcpaComplianceText;
        payload["trustedFormToken"] = lead.TrustedFormCertificateUrl;
        if (!string.IsNullOrWhiteSpace(lead.JornayaLeadId)) payload["leadIDToken"] = lead.JornayaLeadId;
        using var response = await _httpClient.PostAsJsonAsync(snapshot.Endpoint + "/ping-post/posts", payload, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        string? status = null, leadId = null;
        try { using var doc = JsonDocument.Parse(body); status = Field(doc.RootElement, "status"); leadId = Field(doc.RootElement, "leadId"); }
        catch (JsonException) { }
        var accepted = response.IsSuccessStatusCode && status == "success" && !string.IsNullOrWhiteSpace(leadId);
        var unknown = !accepted && ((int)response.StatusCode >= 500 || status is not ("rejected" or "error"));
        return new(accepted, false, unknown, accepted ? leadId : null, AuctionAudit.Payload(payload),
            AuctionAudit.Response(accepted, status: (int)response.StatusCode),
            accepted ? null : unknown ? "Modernize post outcome unknown; reconcile before any retry." : "Modernize rejected the post.",
            (int)response.StatusCode);
    }

    private static AuctionOffer RejectPing(string reason) => new(false, null, null, null, "{}", "{}", reason);
    private static AuctionPost RejectPost(string reason) => new(false, false, false, null, "{}", "{}", reason);
    private static string? Field(JsonElement root, string key) => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(key, out var v)
        ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.ValueKind == JsonValueKind.Number ? v.GetRawText() : null : null;

    public static Dictionary<string, object?>? AuctionPayload(AuctionFacts facts)
    {
        if (facts.OwnsProperty == null || facts.Zip.Length != 5 || !facts.Zip.All(char.IsAsciiDigit)) return null;
        using var doc = JsonDocument.Parse(facts.Qualifications);
        string Value(params string[] keys) => keys.Select(k => Field(doc.RootElement, k)).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? "";
        string Norm(string value) => new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
        var project = Norm(Value("projectType", "roofingProject", "windowProject", "bathroomProject", "hvacProject"));
        var timeframe = Value("purchaseTimeFrame", "timeframe");
        var payload = new Dictionary<string, object?> {
            ["postalCode"] = facts.Zip, ["ownHome"] = facts.OwnsProperty == true ? "Yes" : "No",
            ["publisherSubId"] = $"HOMEYY-{facts.LeadId}",
            ["buyTimeframe"] = timeframe.Contains("immediate", StringComparison.OrdinalIgnoreCase) ? "Immediately" :
                timeframe.Contains("month", StringComparison.OrdinalIgnoreCase) ? "1-6 months" : "Don't know"
        };
        var repair = project.Contains("repair");
        var install = project.Contains("install") || project.Contains("replace");
        switch (facts.Vertical)
        {
            case "Roofing":
                var material = Norm(Value("roofingType", "roofType", "roofingMaterial", "material"));
                var service = material switch {
                    var x when x.Contains("asphalt") => "ROOFING_ASPHALT",
                    var x when x.Contains("metal") => "ROOFING_METAL",
                    var x when x.Contains("tile") => "ROOFING_TILE",
                    var x when x.Contains("cedar") => "ROOFING_CEDAR_SHAKE",
                    var x when x.Contains("slate") => "ROOFING_NATURAL_SLATE",
                    var x when x.Contains("tar") => "ROOFING_TAR_TORCHDOWN",
                    var x when x.Contains("composite") => "ROOFING_COMPOSITE", _ => null };
                if (service == null || !repair && !install) return null;
                payload["service"] = service;
                payload["RoofingPlan"] = repair ? "Repair existing roof" : project.Contains("replace") ? "Completely replace roof" : "Install roof on new construction";
                break;
            case "Windows":
                var count = Value("windowCount", "numWindows", "windowsCount", "windowQuantity");
                var range = count switch { "1" => "1", "2" => "2", "3-5" => "3-5", "6-9" or "10+" => "6-9", _ => null };
                if (range == null && int.TryParse(count, out var n) && n > 0) range = n <= 2 ? n.ToString() : n <= 5 ? "3-5" : "6-9";
                if (range == null || !repair && !install) return null;
                payload["service"] = "WINDOWS"; payload["NumberOfWindows"] = range; payload["WindowsProjectScope"] = repair ? "Repair" : "Install";
                break;
            case "Bathroom":
                payload["service"] = "BATH_REMODEL";
                break;
            case "HVAC":
                var system = Norm(Value("systemType", "airType"));
                if (!repair && !install) return null;
                var boiler = system.Contains("boiler"); var heating = boiler || system.Contains("furnace") || system.Contains("heating");
                if (!heating && !system.Contains("centralac") && !system.Contains("centralair")) return null;
                payload["service"] = "HVAC";
                payload["HVACInterest"] = (repair ? "Repair " : "Install ") + (boiler ? "Boiler/Radiator" : heating ? "Central Heating" : "Central AC");
                if (heating) {
                    var fuel = system.Contains("propane") ? "PropaneGas" : system.Contains("gas") ? "NaturalGas" : system.Contains("oil") ? "Oil" : system.Contains("electric") ? "Electric" : null;
                    if (fuel == null) return null;
                    payload[(boiler ? "BoilerSystem" : "CentralHeating") + (repair ? "RepairType" : "InstallType")] = fuel;
                }
                break;
            default: return null;
        }
        return payload;
    }

    private static string? UsState(string? value)
    {
        const string names = "Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|District of Columbia|Florida|Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|New Hampshire|New Jersey|New Mexico|New York|North Carolina|North Dakota|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode Island|South Carolina|South Dakota|Tennessee|Texas|Utah|Vermont|Virginia|Washington|West Virginia|Wisconsin|Wyoming";
        var codes = "AL AK AZ AR CA CO CT DE DC FL GA HI ID IL IN IA KS KY LA ME MD MA MI MN MS MO MT NE NV NH NJ NM NY NC ND OH OK OR PA RI SC SD TN TX UT VT VA WA WV WI WY".Split(' ');
        var text = value?.Trim();
        if (codes.Contains(text?.ToUpperInvariant())) return text!.ToUpperInvariant();
        var index = Array.FindIndex(names.Split('|'), n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? null : codes[index];
    }
}
