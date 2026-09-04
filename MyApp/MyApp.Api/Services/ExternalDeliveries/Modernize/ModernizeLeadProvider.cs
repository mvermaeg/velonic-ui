using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries.Modernize
{
    public sealed class ModernizeLeadProvider
        : IExternalLeadProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ModernizeOptions _options;
        private readonly ILogger<ModernizeLeadProvider> _logger;

        public ModernizeLeadProvider(
            HttpClient httpClient,
            IOptions<ModernizeOptions> options,
            ILogger<ModernizeLeadProvider> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public string PlatformCode => "Modernize";

        public async Task<ExternalLeadDeliveryResult> DeliverAsync(
            Lead lead,
            ExternalLeadDelivery delivery,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return ExternalLeadDeliveryResult.Failed(
                    false, null, null, null,
                    "Modernize integration is disabled.");
            }

            var validationError = ValidateLead(lead);

            if (validationError != null)
            {
                return ExternalLeadDeliveryResult.Failed(
                    false, null, null, null,
                    validationError);
            }

            var service = ResolveService(lead);

            if (service == null)
            {
                return ExternalLeadDeliveryResult.Failed(
                    false, null, null, null,
                    $"Unsupported Modernize LeadTypeId: {lead.LeadTypeId}");
            }

            var baseUrl =
                _options.UseStaging
                    ? _options.StagingBaseUrl
                    : _options.ProductionBaseUrl;

            var tagId =
                _options.UseStaging
                    ? _options.StagingTagId
                    : _options.ProductionTagId;

            if (string.IsNullOrWhiteSpace(tagId))
            {
                return ExternalLeadDeliveryResult.Failed(
                    false, null, null, null,
                    "Modernize tagId is missing.");
            }

            try
            {
                var pingPayload =
                    BuildPingPayload(
                        lead,
                        service,
                        tagId);

                var pingJson =
                    JsonSerializer.Serialize(
                        pingPayload);

                using var pingResponse =
                    await _httpClient.PostAsJsonAsync(
                        $"{baseUrl.TrimEnd('/')}/ping-post/pings",
                        pingPayload,
                        cancellationToken);

                var pingResponseJson =
                    await pingResponse.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                if (!pingResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        IsRetryable(
                            (int)pingResponse.StatusCode),
                        (int)pingResponse.StatusCode,
                        pingJson,
                        pingResponseJson,
                        "Modernize Ping HTTP request failed.");
                }

                var pingToken =
                    ExtractString(
                        pingResponseJson,
                        "pingToken");

                if (string.IsNullOrWhiteSpace(
                        pingToken))
                {
                    return ExternalLeadDeliveryResult.Failed(
                        false,
                        (int)pingResponse.StatusCode,
                        pingJson,
                        pingResponseJson,
                        "Modernize Ping did not return pingToken.");
                }

                var postPayload =
                    BuildPostPayload(
                        lead,
                        service,
                        tagId,
                        pingToken);

                var postJson =
                    JsonSerializer.Serialize(
                        postPayload);

                using var postResponse =
                    await _httpClient.PostAsJsonAsync(
                        $"{baseUrl.TrimEnd('/')}/ping-post/posts",
                        postPayload,
                        cancellationToken);

                var postResponseJson =
                    await postResponse.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                if (!postResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        IsRetryable(
                            (int)postResponse.StatusCode),
                        (int)postResponse.StatusCode,
                        postJson,
                        postResponseJson,
                        "Modernize Post failed.");
                }

                var reference =
                    ExtractString(
                        postResponseJson,
                        "leadId")
                    ??
                    ExtractString(
                        postResponseJson,
                        "id")
                    ??
                    pingToken;

                return ExternalLeadDeliveryResult.Success(
                    (int)postResponse.StatusCode,
                    reference,
                    postJson,
                    postResponseJson);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Modernize timeout LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    true,
                    null,
                    null,
                    null,
                    "Modernize request timed out.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Modernize delivery failed LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    false,
                    null,
                    null,
                    null,
                    ex.Message);
            }
        }

        private object BuildPingPayload(
            Lead lead,
            string service,
            string tagId)
        {
            var payload =
                new Dictionary<string, object?>
                {
                    ["tagId"] = tagId,
                    ["service"] = service,
                    ["postalCode"] = lead.Postcode,
                    ["buyTimeframe"] =
                        ResolveTimeframe(lead),
                    ["ownHome"] =
                        lead.OwnsProperty == true
                            ? "Yes"
                            : "No",
                    ["partnerSourceId"] =
                        _options.PartnerSourceId,
                    ["publisherSubId"] =
                        $"HOMEYY-{lead.Id}"
                };

            AddServiceFields(
                payload,
                lead,
                service);

            return payload;
        }

        private object BuildPostPayload(
            Lead lead,
            string service,
            string tagId,
            string pingToken)
        {
            var (firstName, lastName) =
                SplitName(
                    lead.FullName);

            var payload =
                new Dictionary<string, object?>
                {
                    ["tagId"] = tagId,
                    ["service"] = service,
                    ["postalCode"] = lead.Postcode,
                    ["buyTimeframe"] =
                        ResolveTimeframe(lead),
                    ["ownHome"] =
                        lead.OwnsProperty == true
                            ? "Yes"
                            : "No",
                    ["partnerSourceId"] =
                        _options.PartnerSourceId,
                    ["publisherSubId"] =
                        $"HOMEYY-{lead.Id}",
                    ["pingToken"] =
                        pingToken,

                    ["firstName"] =
                        firstName,
                    ["lastName"] =
                        lastName,
                    ["address"] =
                        lead.Address,
                    ["city"] =
                        lead.City,
                    ["state"] =
                        lead.State,
                    ["phone"] =
                        NormalizePhone(
                            lead.Phone),
                    ["email"] =
                        lead.Email,

                    ["homePhoneConsentLanguage"] =
                        lead.TcpaComplianceText,

                    ["trustedFormToken"] =
                        lead.TrustedFormCertificateUrl
                };

            if (!string.IsNullOrWhiteSpace(
                    lead.JornayaLeadId))
            {
                payload["leadIDToken"] =
                    lead.JornayaLeadId;
            }

            AddServiceFields(
                payload,
                lead,
                service);

            return payload;
        }

        private static void AddServiceFields(
            Dictionary<string, object?> payload,
            Lead lead,
            string service)
        {
            var additional =
                ParseAdditionalData(
                    lead.AdditionalDataJson);

            switch (service)
            {
                case "WINDOWS":

                    payload["NumberOfWindows"] =
                        ResolveModernizeWindowCount(
                            additional);

                    payload["WindowsProjectScope"] =
                        "Install";

                    break;

                case "HVAC":

                    payload["HVACInterest"] =
                        ResolveHvacInterest(
                            additional);

                    break;

                case "ROOFING_ASPHALT":

                    payload["RoofingPlan"] =
                        ResolveRoofingPlan(
                            additional);

                    break;

                case "BATH_REMODEL":

                    // Modernize lists OptIn1 as optional.
                    break;
            }
        }

        private static string? ResolveService(
            Lead lead)
        {
            return lead.LeadTypeId switch
            {
                1 => "ROOFING_ASPHALT",
                2 => "WINDOWS",
                4 => "HVAC",
                5 => "BATH_REMODEL",
                _ => null
            };
        }

        private static string ResolveModernizeWindowCount(
            Dictionary<string, JsonElement> data)
        {
            var count =
                GetInt(
                    data,
                    "numWindows")
                ??
                GetInt(
                    data,
                    "windowsCount")
                ??
                1;

            if (count <= 1)
                return "1";

            if (count == 2)
                return "2";

            if (count <= 5)
                return "3-5";

            return "6-9";
        }

        private static string ResolveHvacInterest(
            Dictionary<string, JsonElement> data)
        {
            var text =
                GetString(
                    data,
                    "projectType")
                ??
                string.Empty;

            if (text.Contains(
                    "repair",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Repair Central AC";
            }

            return "Install Central AC";
        }

        private static string ResolveRoofingPlan(
            Dictionary<string, JsonElement> data)
        {
            var text =
                GetString(
                    data,
                    "projectType")
                ??
                string.Empty;

            if (text.Contains(
                    "repair",
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Repair existing roof";
            }

            return
                "Completely replace roof";
        }

        private static string ResolveTimeframe(
            Lead lead)
        {
            if (string.IsNullOrWhiteSpace(
                    lead.AdditionalDataJson))
            {
                return "Don't know";
            }

            try
            {
                using var document =
                    JsonDocument.Parse(
                        lead.AdditionalDataJson);

                var root =
                    document.RootElement;

                if (root.TryGetProperty(
                        "timeFrame",
                        out var field) ||
                    root.TryGetProperty(
                        "timeframe",
                        out field))
                {
                    var value =
                        field.GetString()
                        ??
                        string.Empty;

                    if (value.Contains(
                            "immediate",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return "Immediately";
                    }

                    if (value.Contains(
                            "month",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return "1-6 months";
                    }
                }
            }
            catch
            {
            }

            return "Don't know";
        }

        private static string? ValidateLead(
            Lead lead)
        {
            if (string.IsNullOrWhiteSpace(
                    lead.Postcode))
                return "Postcode is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.FullName))
                return "FullName is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Email))
                return "Email is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Phone))
                return "Phone is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Address))
                return "Address is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.City))
                return "City is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.State))
                return "State is required.";

            // Modernize requires TrustedForm.
            if (string.IsNullOrWhiteSpace(
                    lead.TrustedFormCertificateUrl))
            {
                return
                    "Modernize requires TrustedFormCertificateUrl.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.TcpaComplianceText))
            {
                return
                    "Modernize requires TCPA consent language.";
            }

            return null;
        }

        private static (
            string First,
            string Last)
            SplitName(string? value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            var parts =
                value.Trim()
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return (
                    parts[0],
                    parts[0]);
            }

            return (
                parts[0],
                string.Join(
                    " ",
                    parts.Skip(1)));
        }

        private static string NormalizePhone(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(
                    phone))
                return string.Empty;

            var result =
                new string(
                    phone.Where(
                            char.IsDigit)
                        .ToArray());

            if (result.Length == 11 &&
                result.StartsWith("1"))
            {
                result =
                    result.Substring(1);
            }

            return result;
        }

        private static string? ExtractString(
            string json,
            string property)
        {
            try
            {
                using var doc =
                    JsonDocument.Parse(
                        json);

                if (doc.RootElement
                    .TryGetProperty(
                        property,
                        out var value))
                {
                    return value.ToString();
                }
            }
            catch
            {
            }

            return null;
        }

        private static Dictionary<
            string,
            JsonElement>
            ParseAdditionalData(
                string? json)
        {
            var result =
                new Dictionary<
                    string,
                    JsonElement>(
                    StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(
                    json))
            {
                return result;
            }

            try
            {
                using var doc =
                    JsonDocument.Parse(json);

                foreach (
                    var property
                    in doc.RootElement
                        .EnumerateObject())
                {
                    result[property.Name] =
                        property.Value.Clone();
                }
            }
            catch
            {
            }

            return result;
        }

        private static int? GetInt(
            Dictionary<string, JsonElement> data,
            string key)
        {
            if (!data.TryGetValue(
                    key,
                    out var value))
                return null;

            if (value.ValueKind ==
                    JsonValueKind.Number &&
                value.TryGetInt32(
                    out var intValue))
            {
                return intValue;
            }

            if (value.ValueKind ==
                    JsonValueKind.String &&
                int.TryParse(
                    value.GetString(),
                    out intValue))
            {
                return intValue;
            }

            return null;
        }

        private static string? GetString(
            Dictionary<string, JsonElement> data,
            string key)
        {
            if (!data.TryGetValue(
                    key,
                    out var value))
                return null;

            return value.ToString();
        }

        private static bool IsRetryable(
            int status)
        {
            return status == 408 ||
                   status == 429 ||
                   status >= 500;
        }
    }
}