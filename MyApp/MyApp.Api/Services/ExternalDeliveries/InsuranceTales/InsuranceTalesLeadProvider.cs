using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries.InsuranceTales
{
    public sealed class InsuranceTalesLeadProvider
        : IExternalLeadProvider
    {
        private readonly HttpClient _httpClient;
        private readonly InsuranceTalesOptions _options;
        private readonly ILogger<InsuranceTalesLeadProvider> _logger;

        public InsuranceTalesLeadProvider(
            HttpClient httpClient,
            IOptions<InsuranceTalesOptions> options,
            ILogger<InsuranceTalesLeadProvider> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public string PlatformCode => "InsuranceTales";

        public async Task<ExternalLeadDeliveryResult> DeliverAsync(
            Lead lead,
            ExternalLeadDelivery delivery,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "InsuranceTales integration is disabled.");
            }

            if (!_options.Windows.Enabled)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "InsuranceTales Windows campaign is disabled.");
            }

            if (string.IsNullOrWhiteSpace(
                    _options.Windows.CampaignId) ||
                string.IsNullOrWhiteSpace(
                    _options.Windows.CampaignKey))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        "InsuranceTales campaign credentials are missing.");
            }

            // Homeyy LeadTypeId:
            // 1 = Roofing
            // 2 = Windows
            // 4 = HVAC
            // 5 = Bathroom
            if (lead.LeadTypeId != 2)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        $"InsuranceTales currently accepts Windows leads only. LeadTypeId={lead.LeadTypeId}");
            }

            var validationError =
                ValidateLeadForInsuranceTales(lead);

            if (validationError != null)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: validationError);
            }

            try
            {
                /*
                 * =========================================
                 * STEP 1: PING
                 * =========================================
                 */

                var windowsCount =
                    GetWindowsCount(lead);

                var pingFields =
                    BuildPingFields(
                        lead,
                        windowsCount);

                var safePingPayload =
                    SerializeSafePayload(pingFields);

                _logger.LogInformation(
                    "INSURANCETALES PING | LeadId={LeadId} | Payload={Payload}",
                    lead.Id,
                    safePingPayload);

                using var pingContent =
                    new FormUrlEncodedContent(pingFields);

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
                    "INSURANCETALES PING RESPONSE | LeadId={LeadId} | Status={StatusCode} | Response={Response}",
                    lead.Id,
                    (int)pingHttpResponse.StatusCode,
                    pingResponseText);

                if (!pingHttpResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        isRetryable:
                            IsRetryableHttpStatus(
                                (int)pingHttpResponse.StatusCode),
                        httpStatusCode:
                            (int)pingHttpResponse.StatusCode,
                        requestPayload:
                            safePingPayload,
                        responsePayload:
                            pingResponseText,
                        errorMessage:
                            "InsuranceTales Ping HTTP request failed.");
                }

                var pingResult =
                    ParsePingResponse(
                        pingResponseText);

                if (!pingResult.Success)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        isRetryable: false,
                        httpStatusCode:
                            (int)pingHttpResponse.StatusCode,
                        requestPayload:
                            safePingPayload,
                        responsePayload:
                            pingResponseText,
                        errorMessage:
                            pingResult.Error ??
                            pingResult.Message ??
                            "InsuranceTales Ping rejected.");
                }

                if (string.IsNullOrWhiteSpace(
                        pingResult.PingId))
                {
                    return ExternalLeadDeliveryResult.Failed(
                        isRetryable: false,
                        httpStatusCode:
                            (int)pingHttpResponse.StatusCode,
                        requestPayload:
                            safePingPayload,
                        responsePayload:
                            pingResponseText,
                        errorMessage:
                            "InsuranceTales Ping was accepted but no ping_id was returned.");
                }

                /*
                 * =========================================
                 * STEP 2: POST
                 * =========================================
                 */

                var postFields =
                    BuildPostFields(
                        lead,
                        pingResult.PingId,
                        windowsCount);

                var safePostPayload =
                    SerializeSafePayload(postFields);

                _logger.LogInformation(
                    "INSURANCETALES POST | LeadId={LeadId} | PingId={PingId} | Payload={Payload}",
                    lead.Id,
                    pingResult.PingId,
                    safePostPayload);

                using var postContent =
                    new FormUrlEncodedContent(postFields);

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
                    "INSURANCETALES POST RESPONSE | LeadId={LeadId} | Status={StatusCode} | Response={Response}",
                    lead.Id,
                    (int)postHttpResponse.StatusCode,
                    postResponseText);

                if (!postHttpResponse.IsSuccessStatusCode)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        isRetryable:
                            IsRetryableHttpStatus(
                                (int)postHttpResponse.StatusCode),
                        httpStatusCode:
                            (int)postHttpResponse.StatusCode,
                        requestPayload:
                            safePostPayload,
                        responsePayload:
                            postResponseText,
                        errorMessage:
                            "InsuranceTales Post HTTP request failed.");
                }

                var postResult =
                    ParsePostResponse(
                        postResponseText);

                if (!postResult.Success)
                {
                    return ExternalLeadDeliveryResult.Failed(
                        isRetryable: false,
                        httpStatusCode:
                            (int)postHttpResponse.StatusCode,
                        requestPayload:
                            safePostPayload,
                        responsePayload:
                            postResponseText,
                        errorMessage:
                            postResult.Error ??
                            postResult.Message ??
                            "InsuranceTales lead was rejected.");
                }

                return ExternalLeadDeliveryResult.Success(
                    httpStatusCode:
                        (int)postHttpResponse.StatusCode,
                    externalReferenceId:
                        postResult.LeadId ??
                        pingResult.PingId,
                    requestPayload:
                        safePostPayload,
                    responsePayload:
                        postResponseText);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "InsuranceTales delivery timed out. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        "InsuranceTales request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "InsuranceTales HTTP request failed. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "InsuranceTales delivery failed. LeadId={LeadId}",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        ex.Message);
            }
        }

        /*
         * =========================================
         * BUILD PING
         * =========================================
         */

        private Dictionary<string, string> BuildPingFields(
            Lead lead,
            int windowsCount)
        {
            var fields =
                new Dictionary<string, string>
                {
                    ["lp_campaign_id"] =
                        _options.Windows.CampaignId,

                    ["lp_campaign_key"] =
                        _options.Windows.CampaignKey,

                    ["ip_address"] =
                        lead.IpAddress ?? string.Empty,

                    ["project_type"] =
                        "windows_replacement",

                    ["windows_count"] =
                        windowsCount.ToString(
                            CultureInfo.InvariantCulture),

                    ["landingpage"] =
                        GetLandingPage(lead)
                };

            if (_options.UseTestMode)
            {
                fields["lp_test"] = "1";
            }

            return fields;
        }

        /*
         * =========================================
         * BUILD POST
         * =========================================
         */

        private Dictionary<string, string> BuildPostFields(
            Lead lead,
            string pingId,
            int windowsCount)
        {
            var (firstName, lastName) =
                SplitName(lead.FullName);

            var fields =
                new Dictionary<string, string>
                {
                    ["lp_campaign_id"] =
                        _options.Windows.CampaignId,

                    ["lp_campaign_key"] =
                        _options.Windows.CampaignKey,

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

                    ["project_type"] =
                        "windows_replacement",

                    ["windows_count"] =
                        windowsCount.ToString(
                            CultureInfo.InvariantCulture),

                    ["landingpage"] =
                        GetLandingPage(lead)
                };

            /*
             * Alpha Living requires:
             *
             * jornaya_lead_id
             * OR
             * trusted_form_cert_id
             */

            if (!string.IsNullOrWhiteSpace(
                    lead.JornayaLeadId))
            {
                fields["jornaya_lead_id"] =
                    lead.JornayaLeadId;
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.TrustedFormCertificateUrl))
            {
                fields["trusted_form_cert_id"] =
                    lead.TrustedFormCertificateUrl;
            }

            if (_options.UseTestMode)
            {
                fields["lp_test"] = "1";
            }

            return fields;
        }

        /*
         * =========================================
         * VALIDATION
         * =========================================
         */

        private static string? ValidateLeadForInsuranceTales(
            Lead lead)
        {
            if (string.IsNullOrWhiteSpace(
                    lead.FullName))
            {
                return "FullName is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Phone))
            {
                return "Phone is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Email))
            {
                return "Email is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Address))
            {
                return "Address is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.City))
            {
                return "City is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.State))
            {
                return "State is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Postcode))
            {
                return "Zip/Postcode is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.IpAddress))
            {
                return "IP address is required.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.LandingPageUrl))
            {
                return "LandingPageUrl is required.";
            }

            /*
             * TCPA requirement from posting instructions:
             *
             * jornaya_lead_id OR trusted_form_cert_id
             */

            if (string.IsNullOrWhiteSpace(
                    lead.JornayaLeadId) &&
                string.IsNullOrWhiteSpace(
                    lead.TrustedFormCertificateUrl))
            {
                return
                    "InsuranceTales requires JornayaLeadId or TrustedFormCertificateUrl.";
            }

            return null;
        }

        /*
         * =========================================
         * COMPLETE SPLIT NAME METHOD
         * =========================================
         */

        private static (
            string FirstName,
            string LastName)
            SplitName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            var parts =
                fullName
                    .Trim()
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            if (parts.Length == 1)
            {
                /*
                 * Alpha Living requires both
                 * first_name and last_name.
                 *
                 * If Homeyy only received one name,
                 * use same value for last name rather
                 * than posting an empty required field.
                 */
                return (
                    parts[0],
                    parts[0]);
            }

            var firstName =
                parts[0];

            var lastName =
                string.Join(
                    " ",
                    parts.Skip(1));

            return (
                firstName,
                lastName);
        }

        /*
         * =========================================
         * PHONE NORMALIZATION
         * =========================================
         */

        private static string NormalizePhone(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return string.Empty;
            }

            var digits =
                new string(
                    phone
                        .Where(char.IsDigit)
                        .ToArray());

            /*
             * If US phone comes as:
             * 1XXXXXXXXXX
             *
             * Leadspedia generally expects
             * a standard 10 digit US number.
             */
            if (digits.Length == 11 &&
                digits.StartsWith("1"))
            {
                digits =
                    digits.Substring(1);
            }

            return digits;
        }

        /*
         * =========================================
         * WINDOWS COUNT
         * =========================================
         */

        private static int GetWindowsCount(
            Lead lead)
        {
            /*
             * Homeyy has previously stored data like:
             *
             * {
             *   "projectType":
             *     "Interested in replacement windows",
             *   "windowMaterial": "Vinyl",
             *   "numWindows": 5
             * }
             */

            if (string.IsNullOrWhiteSpace(
                    lead.AdditionalDataJson))
            {
                return 1;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(
                        lead.AdditionalDataJson);

                var root =
                    document.RootElement;

                if (root.TryGetProperty(
                        "numWindows",
                        out var numWindows))
                {
                    if (numWindows.ValueKind ==
                        JsonValueKind.Number &&
                        numWindows.TryGetInt32(
                            out var numericValue))
                    {
                        return numericValue > 0
                            ? numericValue
                            : 1;
                    }

                    if (numWindows.ValueKind ==
                        JsonValueKind.String &&
                        int.TryParse(
                            numWindows.GetString(),
                            out var stringValue))
                    {
                        return stringValue > 0
                            ? stringValue
                            : 1;
                    }
                }

                if (root.TryGetProperty(
                        "windowsCount",
                        out var windowsCount))
                {
                    if (windowsCount.ValueKind ==
                        JsonValueKind.Number &&
                        windowsCount.TryGetInt32(
                            out var numericValue))
                    {
                        return numericValue > 0
                            ? numericValue
                            : 1;
                    }

                    if (windowsCount.ValueKind ==
                        JsonValueKind.String &&
                        int.TryParse(
                            windowsCount.GetString(),
                            out var stringValue))
                    {
                        return stringValue > 0
                            ? stringValue
                            : 1;
                    }
                }
            }
            catch
            {
                /*
                 * Do not fail entire delivery just
                 * because AdditionalDataJson cannot
                 * be parsed.
                 */
            }

            return 1;
        }

        /*
         * =========================================
         * LANDING PAGE
         * =========================================
         */

        private static string GetLandingPage(
            Lead lead)
        {
            if (!string.IsNullOrWhiteSpace(
                    lead.LandingPageUrl))
            {
                return lead.LandingPageUrl;
            }

            return
                "https://www.homeyy.com/";
        }

        /*
         * =========================================
         * PARSE PING RESPONSE
         * =========================================
         */

        private static InsuranceTalesPingResponse
            ParsePingResponse(
                string responseText)
        {
            var result =
                new InsuranceTalesPingResponse
                {
                    RawResponse =
                        responseText
                };

            if (string.IsNullOrWhiteSpace(
                    responseText))
            {
                result.Success = false;

                result.Error =
                    "Empty Ping response.";

                return result;
            }

            try
            {
                var xml =
                    XDocument.Parse(
                        responseText);

                var root =
                    xml.Root;

                var responseResult =
                    root?
                        .Element("result")?
                        .Value?
                        .Trim();

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

                var priceText =
                    root?
                        .Element("price")?
                        .Value?
                        .Trim();

                if (decimal.TryParse(
                        priceText,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    result.Price =
                        price;
                }

                result.Success =
                    string.Equals(
                        responseResult,
                        "success",
                        StringComparison
                            .OrdinalIgnoreCase);

                if (!result.Success)
                {
                    result.Error =
                        root?
                            .Element("errors")?
                            .Elements("error")
                            .Select(x => x.Value)
                            .FirstOrDefault();

                    result.Error ??=
                        result.Message;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Error =
                    $"Unable to parse InsuranceTales Ping response: {ex.Message}";

                return result;
            }
        }

        /*
         * =========================================
         * PARSE POST RESPONSE
         * =========================================
         */

        private static InsuranceTalesPostResponse
            ParsePostResponse(
                string responseText)
        {
            var result =
                new InsuranceTalesPostResponse
                {
                    RawResponse =
                        responseText
                };

            if (string.IsNullOrWhiteSpace(
                    responseText))
            {
                result.Success = false;

                result.Error =
                    "Empty Post response.";

                return result;
            }

            try
            {
                var xml =
                    XDocument.Parse(
                        responseText);

                var root =
                    xml.Root;

                var responseResult =
                    root?
                        .Element("result")?
                        .Value?
                        .Trim();

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

                result.RedirectUrl =
                    root?
                        .Element("redirect_url")?
                        .Value?
                        .Trim();

                var priceText =
                    root?
                        .Element("price")?
                        .Value?
                        .Trim();

                if (decimal.TryParse(
                        priceText,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var price))
                {
                    result.Price =
                        price;
                }

                result.Success =
                    string.Equals(
                        responseResult,
                        "success",
                        StringComparison
                            .OrdinalIgnoreCase);

                if (!result.Success)
                {
                    result.Error =
                        root?
                            .Element("errors")?
                            .Elements("error")
                            .Select(x => x.Value)
                            .FirstOrDefault();

                    result.Error ??=
                        result.Message;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;

                result.Error =
                    $"Unable to parse InsuranceTales Post response: {ex.Message}";

                return result;
            }
        }

        /*
         * =========================================
         * SAFE LOGGING
         * =========================================
         */

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

        /*
         * =========================================
         * RETRY DECISION
         * =========================================
         */

        private static bool IsRetryableHttpStatus(
            int statusCode)
        {
            return statusCode == 408 ||
                   statusCode == 429 ||
                   statusCode >= 500;
        }
    }
}