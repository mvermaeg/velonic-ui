using System.Globalization;
using System.Text.Json;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Routing;

public sealed class ExternalLeadAuctionOptions
{
    public bool Enabled { get; set; }
    public int AuctionWindowSeconds { get; set; } = 90;
    public int PostTimeoutSeconds { get; set; } = 45;
    public int MaxWinnerPostAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public bool AllowWinnerFallback { get; set; } = false;
    public string ThumbtackErrorPolicy { get; set; } = "FailClosed";
    public string? KeyRingPath { get; set; }
    public static bool Valid(ExternalLeadAuctionOptions x) => !x.AllowWinnerFallback &&
        x.AuctionWindowSeconds is >= 1 and <= 300 && x.PostTimeoutSeconds is >= 1 and <= 120 &&
        x.MaxWinnerPostAttempts is >= 1 and <= 10 && x.RetryDelaySeconds is >= 1 and <= 86400 &&
        x.ThumbtackErrorPolicy is "FailClosed" or "FallThrough" &&
        (!x.Enabled || !string.IsNullOrWhiteSpace(x.KeyRingPath) && Path.IsPathFullyQualified(x.KeyRingPath));
}

// No contact fields or arbitrary project JSON cross the ping boundary.
public sealed record AuctionFacts(long LeadId, int? LeadTypeId, string Vertical, string Zip,
    string? State, string? City, bool? OwnsProperty, bool IsTest, string Qualifications, string? IpAddress = null,
    string? LandingPageOrigin = null, DateTime CreatedAt = default, string? ConsentText = null)
{
    private static readonly HashSet<string> Keys = new(StringComparer.OrdinalIgnoreCase) {
        "projectType", "roofType", "roofingType", "roofingMaterial", "serviceType", "windowsCount", "numWindows", "windowQuantity",
        "numberOfWindows", "windowCount", "windowProject", "roofingProject", "bathroomProject",
        "hvacProject", "airType", "systemType", "ownProperty", "propertyType", "timeframe",
        "material", "windowMaterial", "creditRating", "bestCallTime", "purchaseTimeFrame" };
    public static AuctionFacts From(Lead lead, string vertical)
    {
        var safe = new Dictionary<string, JsonElement>();
        try
        {
            using var doc = JsonDocument.Parse(lead.AdditionalDataJson ?? "{}");
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
                foreach (var p in doc.RootElement.EnumerateObject().Concat(
                    doc.RootElement.TryGetProperty("answers", out var answers) && answers.ValueKind == JsonValueKind.Object
                        ? answers.EnumerateObject() : Enumerable.Empty<JsonProperty>()))
                    if (Keys.Contains(p.Name) && (p.Value.ValueKind is JsonValueKind.True or JsonValueKind.False ||
                        p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetInt32(out var n) && n is >= 0 and <= 10000 ||
                        p.Value.ValueKind == JsonValueKind.String && p.Value.GetString() is { Length: <= 60 } value &&
                        value.Count(char.IsDigit) <= 3 && value.All(c => char.IsLetterOrDigit(c) || c is ' ' or '_' or '-' or '/' or '+') &&
                        !new[] { lead.FullName, lead.Email, lead.Phone, lead.Address }.Any(s => !string.IsNullOrWhiteSpace(s) && value.Contains(s, StringComparison.OrdinalIgnoreCase))))
                        safe[p.Name] = p.Value.Clone();
        }
        catch (JsonException) { }
        string? Location(string? value) => value != null && value.Length <= 100 &&
            value.All(c => char.IsLetter(c) || c is ' ' or '-' or '.') &&
            !new[] { lead.FullName, lead.Email, lead.Phone, lead.Address }.Any(s => !string.IsNullOrWhiteSpace(s) && value.Contains(s, StringComparison.OrdinalIgnoreCase)) ? value : null;
        return new(lead.Id, lead.LeadTypeId, vertical,
            Thumbtack.ThumbtackCoverageLookup.NormalizeZip(lead.Postcode) ?? "", Location(lead.State), Location(lead.City),
            lead.OwnsProperty, lead.IsTest, JsonSerializer.Serialize(safe), System.Net.IPAddress.TryParse(lead.IpAddress, out var ip) ? ip.ToString() : null,
            Uri.TryCreate(lead.LandingPageUrl, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" ? uri.GetLeftPart(UriPartial.Authority) : null,
            lead.CreatedAt, new[] { lead.FullName, lead.Email, lead.Phone, lead.Address }.Any(s =>
                !string.IsNullOrWhiteSpace(s) && lead.TcpaComplianceText?.Contains(s, StringComparison.OrdinalIgnoreCase) == true) ? null : lead.TcpaComplianceText);
    }
    // Only legacy mapping helpers receive this deliberately contact-free object.
    public Lead ToMappingLead() => new() { Id = LeadId, LeadTypeId = LeadTypeId, Postcode = Zip,
        State = State, City = City, OwnsProperty = OwnsProperty, IsTest = IsTest, AdditionalDataJson = Qualifications,
        IpAddress = IpAddress, LandingPageUrl = LandingPageOrigin, CreatedAt = CreatedAt, TcpaComplianceText = ConsentText };
}

public sealed record AuctionOffer(bool Accepted, decimal? Amount, string? Reference, DateTime? ExpiresOn,
    string RequestAudit, string ResponseAudit, string? Error = null, string? PostContext = null);
public sealed record AuctionPost(bool Accepted, bool RetrySafe, bool OutcomeUnknown, string? DeliveryId,
    string RequestAudit, string ResponseAudit, string? Error = null, int? HttpStatus = null);
public interface IAuctionProvider
{
    string PlatformCode { get; }
    bool IsEnabled(string vertical);
    // True only when a documented remote idempotency contract exists.
    bool SupportsIdempotentPost => false;
    Task<AuctionOffer> PingAsync(AuctionFacts facts, CancellationToken ct);
    Task<AuctionPost> PostAsync(Lead lead, string vertical, string reference, string? context, string idempotencyKey, CancellationToken ct);
}

public static class AuctionPolicy
{
    public static bool HttpsEndpoint(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme == "https" && string.IsNullOrEmpty(uri.UserInfo);
    public static string? Reject(AuctionOffer offer, DateTime received, DateTime closes, DateTime now) =>
        received > closes ? "LateResponse" : !offer.Accepted ? "Rejected" : offer.Amount is null or <= 0 or > 99999999999999.9999m ||
        decimal.Round(offer.Amount.Value, 4) != offer.Amount ? "InvalidBid" :
        string.IsNullOrWhiteSpace(offer.Reference) ? "MissingPostReference" : offer.ExpiresOn <= now ? "ExpiredBid" : null;
    public static LeadRoutingAttempt? Winner(IEnumerable<LeadRoutingAttempt> bids, DateTime closes, DateTime now) =>
        bids.Where(b => b.Status == "BidAccepted" && b.BidReceivedOn <= closes && b.OfferedBidAmount > 0 &&
            !string.IsNullOrWhiteSpace(b.PingReferenceId) && (b.BidExpiresOn == null || b.BidExpiresOn > now))
            .OrderByDescending(b => b.OfferedBidAmount).ThenBy(b => b.BidReceivedOn).ThenBy(b => b.Id).FirstOrDefault();
    public static decimal? Price(string? value) => decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
        CultureInfo.InvariantCulture, out var price) ? price : null;
    public static bool CanRetry(AuctionPost post, bool idempotent) => !post.Accepted && post.RetrySafe && (!post.OutcomeUnknown || idempotent);
}

// Audit is an allowlist, never raw vendor bodies which may echo contacts or credentials.
public static class AuctionAudit
{
    private static readonly HashSet<string> PublicFields = new(StringComparer.OrdinalIgnoreCase) {
        "zipcode", "zip_code", "postalCode", "task_id", "service", "project_type", "Project", "windows_count", "num_windows",
        "roofing_type", "material", "air_type", "system_type", "own_property", "home_owner", "lp_test", "type",
        "credit_rating", "best_call_time", "purchase_time_frame" };
    public static string Payload(object payload)
    {
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload));
        object? Clean(JsonElement element, string key) => element.ValueKind switch {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => Clean(p.Value, p.Name)),
            JsonValueKind.Array => element.EnumerateArray().Select(x => Clean(x, key)).ToArray(),
            JsonValueKind.Null => null,
            _ => PublicFields.Contains(key) && (element.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False ||
                new[] { "zipcode", "zip_code", "postalCode" }.Contains(key) && element.ValueKind == JsonValueKind.String &&
                element.GetString() is { Length: 5 } zip && zip.All(c => c is >= '0' and <= '9'))
                ? element.Clone() : (object)"[REDACTED]"
        };
        return JsonSerializer.Serialize(Clean(doc.RootElement, ""));
    }
    public static string Request(string platform, string operation, long leadId) =>
        JsonSerializer.Serialize(new { platform, operation, leadId, contact = "[REDACTED]", credentials = "[REDACTED]" });
    public static string Response(bool accepted, decimal? amount = null, int? status = null) =>
        JsonSerializer.Serialize(new { accepted, amount, httpStatus = status, rawValues = "[REDACTED]" });
}
