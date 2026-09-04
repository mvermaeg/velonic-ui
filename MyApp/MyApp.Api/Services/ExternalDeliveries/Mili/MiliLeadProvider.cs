using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries.Mili
{
    public sealed class MiliLeadProvider
        : IExternalLeadProvider
    {
        private readonly HttpClient _httpClient;
        private readonly MiliOptions _options;
        private readonly MiliRateLimiter _rateLimiter;
        private readonly ILogger<MiliLeadProvider> _logger;

        public MiliLeadProvider(
            HttpClient httpClient,
            IOptions<MiliOptions> options,
            MiliRateLimiter rateLimiter,
            ILogger<MiliLeadProvider> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _rateLimiter = rateLimiter;
            _logger = logger;
        }

        public string PlatformCode => "Mili";

        public async Task<ExternalLeadDeliveryResult> DeliverAsync(
            Lead lead,
            ExternalLeadDelivery delivery,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return Failed(
                    "MILI integration is disabled.");
            }

            var vertical =
                ResolveVertical(lead.LeadTypeId);

            if (vertical == null)
            {
                return Failed(
                    $"MILI does not support LeadTypeId {lead.LeadTypeId}.");
            }

            var campaign =
                ResolveCampaign(vertical);

            if (campaign == null ||
                !campaign.Enabled)
            {
                return Failed(
                    $"MILI {vertical} campaign is disabled.");
            }

            if (string.IsNullOrWhiteSpace(
                    campaign.CampaignId) ||
                string.IsNullOrWhiteSpace(
                    campaign.CampaignKey))
            {
                return Failed(
                    $"MILI {vertical} campaign credentials are missing.");
            }

            var validation =
                ValidateLead(lead);

            if (validation != null)
            {
                return Failed(validation);
            }

            if (!IsWithinMiliSchedule(
                    out var scheduleReason))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: scheduleReason);
            }

            if (!_rateLimiter.TryAcquire(
                    vertical,
                    out var rateReason))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: rateReason);
            }

            try
            {
                /*
                 * =====================================
                 * PING
                 * =====================================
                 */

                var project =
                    ResolveProject(
                        lead,
                        vertical);

                var pingFields =
                    BuildPingFields(
                        lead,
                        campaign,
                        project);

                var safePingPayload =
                    SerializeSafePayload(
                        pingFields);

                _logger.LogInformation(
                    "MILI PING | LeadId={LeadId} | Vertical={Vertical} | Project={Project} | Payload={Payload}",
                    lead.Id,
                    vertical,
                    project,
                    safePingPayload);

                using var pingContent =
                    new FormUrlEncodedContent(
                        pingFields);

                using var pingHttpResponse =
                    await _httpClient.PostAsync(
                        _options.PingUrl,
                        pingContent,
                        cancellationToken);

                var pingResponseText =
                    await pingHttpResponse.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                _logger.LogInformation(
                    "MILI PING RESPONSE | LeadId={LeadId} | Status={Status} | Response={Response}",
                    lead.Id,
                    (int)pingHttpResponse.StatusCode,
                    pingResponseText);

                if (!pingHttpResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        IsRetryableHttpStatus(
                            (int)pingHttpResponse.StatusCode),
                        (int)pingHttpResponse.StatusCode,
                        safePingPayload,
                        pingResponseText,
                        "MILI Ping HTTP request failed.");
                }

                var pingResult =
                    ParsePingResponse(
                        pingResponseText);

                if (!pingResult.Success)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        false,
                        (int)pingHttpResponse.StatusCode,
                        safePingPayload,
                        pingResponseText,
                        pingResult.Error ??
                        pingResult.Message ??
                        "MILI Ping rejected.");
                }

                if (string.IsNullOrWhiteSpace(
                        pingResult.PingId))
                {
                    return ExternalLeadDeliveryResult.Failed(
                        false,
                        (int)pingHttpResponse.StatusCode,
                        safePingPayload,
                        pingResponseText,
                        "MILI Ping accepted but ping_id was missing.");
                }

                /*
                 * =====================================
                 * POST
                 * =====================================
                 */

                var postFields =
                    BuildPostFields(
                        lead,
                        campaign,
                        pingResult.PingId,
                        project);

                var safePostPayload =
                    SerializeSafePayload(
                        postFields);

                _logger.LogInformation(
                    "MILI POST | LeadId={LeadId} | Vertical={Vertical} | PingId={PingId} | Payload={Payload}",
                    lead.Id,
                    vertical,
                    pingResult.PingId,
                    safePostPayload);

                using var postContent =
                    new FormUrlEncodedContent(
                        postFields);

                using var postHttpResponse =
                    await _httpClient.PostAsync(
                        _options.PostUrl,
                        postContent,
                        cancellationToken);

                var postResponseText =
                    await postHttpResponse.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                _logger.LogInformation(
                    "MILI POST RESPONSE | LeadId={LeadId} | Status={Status} | Response={Response}",
                    lead.Id,
                    (int)postHttpResponse.StatusCode,
                    postResponseText);

                if (!postHttpResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        IsRetryableHttpStatus(
                            (int)postHttpResponse.StatusCode),
                        (int)postHttpResponse.StatusCode,
                        safePostPayload,
                        postResponseText,
                        "MILI Post HTTP request failed.");
                }

                var postResult =
                    ParsePostResponse(
                        postResponseText);

                if (!postResult.Success)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        false,
                        (int)postHttpResponse.StatusCode,
                        safePostPayload,
                        postResponseText,
                        postResult.Error ??
                        postResult.Message ??
                        "MILI lead rejected.");
                }

                return ExternalLeadDeliveryResult.Success(
                    (int)postHttpResponse.StatusCode,
                    postResult.LeadId ??
                    pingResult.PingId,
                    safePostPayload,
                    postResponseText);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken
                    .IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "MILI request timed out. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    true,
                    null,
                    null,
                    null,
                    "MILI request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "MILI HTTP error. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    true,
                    null,
                    null,
                    null,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "MILI delivery error. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    false,
                    null,
                    null,
                    null,
                    ex.Message);
            }
        }

        /*
         * =====================================
         * PING PAYLOAD
         * =====================================
         */

        private Dictionary<string, string>
            BuildPingFields(
                Lead lead,
                MiliVerticalOptions campaign,
                string project)
        {
            var fields =
                new Dictionary<string, string>
                {
                    ["lp_campaign_id"] =
                        campaign.CampaignId,

                    ["lp_campaign_key"] =
                        campaign.CampaignKey,

                    ["city"] =
                        lead.City ?? string.Empty,

                    ["state"] =
                        lead.State ?? string.Empty,

                    ["zip_code"] =
                        lead.Postcode ?? string.Empty,

                    ["ip_address"] =
                        lead.IpAddress ?? string.Empty,

                    ["type"] =
                        NormalizeLeadType(
                            _options.LeadType),

                    ["Project"] =
                        project
                };

            AddCommonOptionalFields(
                fields,
                lead);

            if (_options.UseTestMode)
            {
                fields["lp_test"] = "1";
            }

            return fields;
        }

        /*
         * =====================================
         * POST PAYLOAD
         * =====================================
         */

        private Dictionary<string, string>
            BuildPostFields(
                Lead lead,
                MiliVerticalOptions campaign,
                string pingId,
                string project)
        {
            var (firstName, lastName) =
                SplitName(
                    lead.FullName);

            var fields =
                new Dictionary<string, string>
                {
                    ["lp_campaign_id"] =
                        campaign.CampaignId,

                    ["lp_campaign_key"] =
                        campaign.CampaignKey,

                    ["lp_ping_id"] =
                        pingId,

                    ["first_name"] =
                        firstName,

                    ["last_name"] =
                        lastName,

                    ["phone_home"] =
                        NormalizePhone(
                            lead.Phone),

                    ["address"] =
                        lead.Address ?? string.Empty,

                    ["city"] =
                        lead.City ?? string.Empty,

                    ["state"] =
                        lead.State ?? string.Empty,

                    ["zip_code"] =
                        lead.Postcode ?? string.Empty,

                    ["email_address"] =
                        lead.Email ?? string.Empty,

                    ["ip_address"] =
                        lead.IpAddress ?? string.Empty,

                    ["type"] =
                        NormalizeLeadType(
                            _options.LeadType),

                    ["Project"] =
                        project,

                    ["tcpa_language"] =
                        lead.TcpaComplianceText ??
                        string.Empty,

                    ["TCPA"] =
                        lead.IsTcpaCompliant == true
                            ? "YES"
                            : "NO"
                };

            AddCommonOptionalFields(
                fields,
                lead);

            if (_options.UseTestMode)
            {
                fields["lp_test"] = "1";
            }

            return fields;
        }

        /*
         * =====================================
         * COMMON OPTIONAL MILI FIELDS
         * =====================================
         */

        private void AddCommonOptionalFields(
            Dictionary<string, string> fields,
            Lead lead)
        {
            if (!string.IsNullOrWhiteSpace(
                    _options.PubId))
            {
                fields["pub_id"] =
                    _options.PubId;
            }

            if (lead.OwnsProperty.HasValue)
            {
                fields["home_owner"] =
                    lead.OwnsProperty.Value
                        ? "YES"
                        : "NO";
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.JornayaLeadId))
            {
                fields["universal_leadid"] =
                    lead.JornayaLeadId;
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.TrustedFormCertificateUrl))
            {
                /*
                 * Posting instructions accept these
                 * TrustedForm fields.
                 */

                fields["trusted_form"] =
                    lead.TrustedFormCertificateUrl;

                fields["trusted_form_cert_url"] =
                    lead.TrustedFormCertificateUrl;
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.LandingPageUrl))
            {
                fields["landing_page"] =
                    lead.LandingPageUrl;
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.UserAgent))
            {
                fields["user_agent"] =
                    lead.UserAgent;
            }
        }

        /*
         * =====================================
         * VERTICAL
         * =====================================
         */

        private static string?
            ResolveVertical(int? leadTypeId)
        {
            return leadTypeId switch
            {
                1 => "Roofing",
                2 => "Windows",
                4 => "HVAC",
                5 => "Bathroom",
                _ => null
            };
        }

        private MiliVerticalOptions?
            ResolveCampaign(string vertical)
        {
            return vertical switch
            {
                "Roofing" =>
                    _options.Roofing,

                "Windows" =>
                    _options.Windows,

                "HVAC" =>
                    _options.HVAC,

                "Bathroom" =>
                    _options.Bathroom,

                _ =>
                    null
            };
        }

        /*
         * =====================================
         * PROJECT MAPPING
         * =====================================
         */

        private static string ResolveProject(
            Lead lead,
            string vertical)
        {
            var projectText =
                GetAdditionalString(
                    lead.AdditionalDataJson,
                    "projectType")
                ??
                string.Empty;

            switch (vertical)
            {
                case "Roofing":

                    if (Contains(
                            projectText,
                            "repair"))
                    {
                        return "Roof Repair";
                    }

                    if (Contains(
                            projectText,
                            "new"))
                    {
                        return "New Roof";
                    }

                    return "Roof Replacement";

                case "Windows":

                    if (Contains(
                            projectText,
                            "repair"))
                    {
                        return "Window Repair";
                    }

                    var count =
                        GetAdditionalInt(
                            lead.AdditionalDataJson,
                            "numWindows")
                        ??
                        GetAdditionalInt(
                            lead.AdditionalDataJson,
                            "windowsCount")
                        ??
                        2;

                    return count <= 1
                        ? "Window Install Single"
                        : "Windows Install Multiple";

                case "HVAC":

                    if (Contains(
                            projectText,
                            "repair"))
                    {
                        return
                            "Central A/C Repair Service";
                    }

                    return
                        "Central A/C Install Replace";

                case "Bathroom":

                    if (Contains(
                            projectText,
                            "walk") ||
                        Contains(
                            projectText,
                            "walking bath"))
                    {
                        return "Walking Bath";
                    }

                    if (Contains(
                            projectText,
                            "shower"))
                    {
                        return
                            "Bathroom Shower Install";
                    }

                    return "Bathroom Remodel";

                default:

                    throw new InvalidOperationException(
                        $"Unsupported MILI vertical {vertical}.");
            }
        }

        /*
         * =====================================
         * VALIDATION
         * =====================================
         */

        private static string?
            ValidateLead(Lead lead)
        {
            if (string.IsNullOrWhiteSpace(
                    lead.FullName))
                return "FullName is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Phone))
                return "Phone is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Email))
                return "Email is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Address))
                return "Address is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.City))
                return "City is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.State))
                return "State is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.Postcode))
                return "Zip code is required.";

            if (string.IsNullOrWhiteSpace(
                    lead.IpAddress))
                return "IP address is required.";

            /*
             * MILI Post requires both.
             */

            if (string.IsNullOrWhiteSpace(
                    lead.TcpaComplianceText))
            {
                return
                    "MILI requires TCPA compliance language.";
            }

            return null;
        }

        /*
         * =====================================
         * MILI SCHEDULE
         *
         * 7 days/week
         * 4:00 AM - 10:30 PM Pacific Time
         * =====================================
         */

        private static bool IsWithinMiliSchedule(
            out string? reason)
        {
            reason = null;

            try
            {
                TimeZoneInfo pacific;

                try
                {
                    // Windows / AWS Windows Server
                    pacific =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "Pacific Standard Time");
                }
                catch
                {
                    // Linux fallback
                    pacific =
                        TimeZoneInfo.FindSystemTimeZoneById(
                            "America/Los_Angeles");
                }

                var nowPacific =
                    TimeZoneInfo.ConvertTime(
                        DateTimeOffset.UtcNow,
                        pacific);

                var current =
                    nowPacific.TimeOfDay;

                var start =
                    new TimeSpan(
                        4,
                        0,
                        0);

                var end =
                    new TimeSpan(
                        22,
                        30,
                        0);

                if (current < start ||
                    current > end)
                {
                    reason =
                        $"MILI is outside posting hours. Current Pacific time: {nowPacific:yyyy-MM-dd HH:mm:ss}. Allowed: 04:00-22:30 Pacific.";

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                reason =
                    $"Unable to determine MILI Pacific posting schedule: {ex.Message}";

                return false;
            }
        }

        /*
         * =====================================
         * XML PING RESPONSE
         * =====================================
         */

        private static MiliPingResult
            ParsePingResponse(
                string response)
        {
            var result =
                new MiliPingResult();

            try
            {
                var xml =
                    XDocument.Parse(
                        response);

                var root =
                    xml.Root;

                var status =
                    root?
                        .Element("result")?
                        .Value?
                        .Trim();

                result.Success =
                    string.Equals(
                        status,
                        "success",
                        StringComparison
                            .OrdinalIgnoreCase);

                result.PingId =
                    root?
                        .Element("ping_id")?
                        .Value?
                        .Trim();

                result.Message =
                    root?
                        .Element("msg")?
                        .Value?
                        .Trim();

                var price =
                    root?
                        .Element("price")?
                        .Value?
                        .Trim();

                if (decimal.TryParse(
                        price,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsedPrice))
                {
                    result.Price =
                        parsedPrice;
                }

                if (!result.Success)
                {
                    result.Error =
                        root?
                            .Element("errors")?
                            .Elements("error")
                            .Select(
                                x => x.Value)
                            .FirstOrDefault();

                    result.Error ??=
                        result.Message;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Error =
                    $"Unable to parse MILI Ping response: {ex.Message}";
            }

            return result;
        }

        /*
         * =====================================
         * XML POST RESPONSE
         * =====================================
         */

        private static MiliPostResult
            ParsePostResponse(
                string response)
        {
            var result =
                new MiliPostResult();

            try
            {
                var xml =
                    XDocument.Parse(
                        response);

                var root =
                    xml.Root;

                var status =
                    root?
                        .Element("result")?
                        .Value?
                        .Trim();

                result.Success =
                    string.Equals(
                        status,
                        "success",
                        StringComparison
                            .OrdinalIgnoreCase);

                result.LeadId =
                    root?
                        .Element("lead_id")?
                        .Value?
                        .Trim();

                result.Message =
                    root?
                        .Element("msg")?
                        .Value?
                        .Trim();

                var price =
                    root?
                        .Element("price")?
                        .Value?
                        .Trim();

                if (decimal.TryParse(
                        price,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsedPrice))
                {
                    result.Price =
                        parsedPrice;
                }

                if (!result.Success)
                {
                    result.Error =
                        root?
                            .Element("errors")?
                            .Elements("error")
                            .Select(
                                x => x.Value)
                            .FirstOrDefault();

                    result.Error ??=
                        result.Message;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Error =
                    $"Unable to parse MILI Post response: {ex.Message}";
            }

            return result;
        }

        /*
         * =====================================
         * UTILITIES
         * =====================================
         */

        private static (
            string First,
            string Last)
            SplitName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(
                    fullName))
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            var parts =
                fullName.Trim()
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
            {
                return string.Empty;
            }

            var digits =
                new string(
                    phone.Where(
                            char.IsDigit)
                        .ToArray());

            if (digits.Length == 11 &&
                digits.StartsWith("1"))
            {
                digits =
                    digits.Substring(1);
            }

            return digits;
        }

        private static string NormalizeLeadType(
            string? type)
        {
            return string.Equals(
                    type,
                    "Exclusive",
                    StringComparison.OrdinalIgnoreCase)
                ? "Exclusive"
                : "Shared";
        }

        private static bool Contains(
            string value,
            string search)
        {
            return value.Contains(
                search,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string?
            GetAdditionalString(
                string? json,
                string property)
        {
            if (string.IsNullOrWhiteSpace(
                    json))
                return null;

            try
            {
                using var doc =
                    JsonDocument.Parse(json);

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

        private static int?
            GetAdditionalInt(
                string? json,
                string property)
        {
            if (string.IsNullOrWhiteSpace(
                    json))
                return null;

            try
            {
                using var doc =
                    JsonDocument.Parse(json);

                if (!doc.RootElement
                    .TryGetProperty(
                        property,
                        out var value))
                {
                    return null;
                }

                if (value.ValueKind ==
                        JsonValueKind.Number &&
                    value.TryGetInt32(
                        out var number))
                {
                    return number;
                }

                if (value.ValueKind ==
                        JsonValueKind.String &&
                    int.TryParse(
                        value.GetString(),
                        out number))
                {
                    return number;
                }
            }
            catch
            {
            }

            return null;
        }

        private static string SerializeSafePayload(
            Dictionary<string, string> fields)
        {
            var safe =
                new Dictionary<string, string>(
                    fields,
                    StringComparer.OrdinalIgnoreCase);

            if (safe.ContainsKey(
                    "lp_campaign_key"))
            {
                safe["lp_campaign_key"] =
                    "***REDACTED***";
            }

            return JsonSerializer.Serialize(
                safe);
        }

        private static bool IsRetryableHttpStatus(
            int status)
        {
            return status == 408 ||
                   status == 429 ||
                   status >= 500;
        }

        private static ExternalLeadDeliveryResult
            Failed(string message)
        {
            return ExternalLeadDeliveryResult.Failed(
                false,
                null,
                null,
                null,
                message);
        }

        private sealed class MiliPingResult
        {
            public bool Success { get; set; }

            public string? PingId { get; set; }

            public decimal? Price { get; set; }

            public string? Message { get; set; }

            public string? Error { get; set; }
        }

        private sealed class MiliPostResult
        {
            public bool Success { get; set; }

            public string? LeadId { get; set; }

            public decimal? Price { get; set; }

            public string? Message { get; set; }

            public string? Error { get; set; }
        }
    }
}