using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace MyApp.Api.Services.ExternalDeliveries.Networx
{
    public class NetworxLeadProvider : IExternalLeadProvider
    {
        public const string ProviderCode = "NETWORX";

        private readonly HttpClient _httpClient;
        private readonly MyAppDbContext _db;
        private readonly NetworxOptions _options;
        private readonly ILogger<NetworxLeadProvider> _logger;

        public NetworxLeadProvider(
            HttpClient httpClient,
            MyAppDbContext db,
            IOptions<NetworxOptions> options,
            ILogger<NetworxLeadProvider> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _options = options.Value;
            _logger = logger;
        }

        public string PlatformCode => ProviderCode;

        public async Task<ExternalLeadDeliveryResult> DeliverAsync(
            Lead lead,
            ExternalLeadDelivery delivery,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return Failed(
                    "Networx integration is disabled.");
            }

            if (string.IsNullOrWhiteSpace(_options.UserId))
            {
                return Failed(
                    "Networx User ID is missing.");
            }

            if (string.IsNullOrWhiteSpace(_options.AccessKey))
            {
                return Failed(
                    "Networx access key is missing.");
            }

            if (string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                return Failed(
                    "Networx BaseUrl is missing.");
            }

            var internalOptionCode = ResolveInternalOptionCode(
     delivery.VerticalCode,
     lead.AdditionalDataJson);

            if (string.IsNullOrWhiteSpace(internalOptionCode))
            {
                return Failed(
                    $"Could not resolve the Networx task option for " +
                    $"LeadId {lead.Id}, vertical '{delivery.VerticalCode}'.");
            }

            var taskMapping = await _db.ExternalPlatformTaskMappings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.PlatformCode == ProviderCode &&
                        x.LeadTypeId == lead.LeadTypeId &&
                        x.InternalOptionCode == internalOptionCode &&
                        x.IsActive,
                    cancellationToken);

            if (taskMapping == null)
            {
                return Failed(
                    $"No active Networx task mapping was found for " +
                    $"LeadTypeId {lead.LeadTypeId} and option " +
                    $"'{internalOptionCode}'.");
            }

            if (string.IsNullOrWhiteSpace(
                    taskMapping.ExternalTaskId))
            {
                return Failed(
                    "The configured Networx task ID is empty.");
            }

            var validationError = ValidateLead(lead);

            if (validationError != null)
            {
                return Failed(validationError);
            }

            try
            {
                if (_options.UsePingPost)
                {
                    return await ExecutePingPostAsync(
                        lead,
                        taskMapping.ExternalTaskId,
                        cancellationToken);
                }

                return await ExecuteDirectPostAsync(
                    lead,
                    taskMapping.ExternalTaskId,
                    cancellationToken);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "Networx request timed out for LeadId {LeadId}.",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "Networx request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Networx HTTP request failed for LeadId {LeadId}.",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Networx error for LeadId {LeadId}.",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: ex.Message);
            }
        }

        private static string? ResolveInternalOptionCode(
    string? verticalCode,
    string? additionalDataJson)
        {
            var data = ParseAdditionalData(additionalDataJson);

            var vertical = NormalizeValue(verticalCode);

            return vertical switch
            {
                "roofing" => ResolveRoofingOption(data),
                "windows" => ResolveWindowsOption(data),
                "hvac" => ResolveHvacOption(data),

                "bathroom" or
                "bathroomremodeling" or
                "bathroomremodel" =>
                    ResolveBathroomOption(data),

                _ => null
            };
        }

        private static string? ResolveRoofingOption(
            Dictionary<string, JsonElement> data)
        {
            var projectType = NormalizeValue(
                GetString(data, "projectType"));

            var roofingType = NormalizeValue(
                GetString(data, "roofingType") ??
                GetString(data, "roofType"));

            if (projectType.Contains("replace") &&
                roofingType.Contains("asphalt"))
            {
                return "ROOF_REPLACE_ASPHALT";
            }

            return null;
        }

        private static string? ResolveWindowsOption(
    Dictionary<string, JsonElement> data)
        {
            var projectType = NormalizeValue(
                GetString(data, "projectType"));

            var quantityText =
                GetString(data, "numWindows") ??
                GetString(data, "windowCount") ??
                GetString(data, "windowQuantity");

            var quantity =
                GetIntegerValue(quantityText);

            if (projectType.Contains("glass") &&
                (projectType.Contains("install") ||
                 projectType.Contains("replace")))
            {
                return "WINDOW_GLASS_INSTALL_REPLACE";
            }

            if (projectType.Contains("repair"))
            {
                return "WINDOW_FRAME_GLASS_REPAIR";
            }

            if (quantity.HasValue &&
                quantity.Value > 1)
            {
                return "WINDOW_INSTALL_MULTIPLE";
            }

            if (projectType.Contains("replace") ||
                projectType.Contains("install"))
            {
                return "WINDOW_INSTALL_SINGLE";
            }

            return null;
        }


        private static int? GetIntegerValue(
    string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (int.TryParse(
                    value,
                    out var number))
            {
                return number;
            }

            var firstNumber = new string(
                value
                    .SkipWhile(c => !char.IsDigit(c))
                    .TakeWhile(char.IsDigit)
                    .ToArray());

            return int.TryParse(
                firstNumber,
                out number)
                    ? number
                    : null;
        }


        private static string? ResolveHvacOption(
    Dictionary<string, JsonElement> data)
        {
            var projectType = NormalizeValue(
                GetString(data, "projectType"));

            var systemType = NormalizeValue(
                GetString(data, "systemType"));

            var airType = NormalizeValue(
                GetString(data, "airType"));

            var combined =
                $"{projectType}{systemType}{airType}";

            var isInstall =
                combined.Contains("install") ||
                combined.Contains("replace") ||
                combined.Contains("newunit");

            var isRepair =
                combined.Contains("repair") ||
                combined.Contains("service") ||
                combined.Contains("maintenance");

            if (combined.Contains("centralac") ||
                combined.Contains("centralair"))
            {
                if (isInstall)
                    return "HVAC_CENTRAL_AC_INSTALL";

                if (isRepair)
                    return "HVAC_CENTRAL_AC_REPAIR";
            }

            if (combined.Contains("heatpump"))
            {
                if (isInstall)
                    return "HVAC_HEAT_PUMP_INSTALL";

                if (isRepair)
                    return "HVAC_HEAT_PUMP_REPAIR";
            }

            if (combined.Contains("furnace"))
            {
                if (isInstall)
                    return "HVAC_FURNACE_INSTALL";

                if (isRepair)
                    return "HVAC_FURNACE_REPAIR";
            }

            return null;
        }


        //private static string? ResolveHvacOption(
        //    Dictionary<string, JsonElement> data)
        //{
        //    var projectType = NormalizeValue(
        //        GetString(data, "projectType"));

        //    var systemType = NormalizeValue(
        //        GetString(data, "systemType"));

        //    var combined = $"{projectType} {systemType}";

        //    if (combined.Contains("central") &&
        //        combined.Contains("air") &&
        //        (combined.Contains("install") ||
        //         combined.Contains("replace")))
        //    {
        //        return "HVAC_CENTRAL_AC_INSTALL";
        //    }

        //    if (combined.Contains("central") &&
        //        combined.Contains("air") &&
        //        (combined.Contains("repair") ||
        //         combined.Contains("service")))
        //    {
        //        return "HVAC_CENTRAL_AC_REPAIR";
        //    }

        //    if (combined.Contains("heatpump") &&
        //        combined.Contains("install"))
        //    {
        //        return "HVAC_HEAT_PUMP_INSTALL";
        //    }

        //    if (combined.Contains("heatpump") &&
        //        combined.Contains("repair"))
        //    {
        //        return "HVAC_HEAT_PUMP_REPAIR";
        //    }

        //    return null;
        //}


        private static string? ResolveBathroomOption(
    Dictionary<string, JsonElement> data)
        {
            var projectType = NormalizeValue(
                GetString(data, "projectType"));

            if (string.IsNullOrWhiteSpace(projectType))
            {
                return null;
            }

            if (projectType.Contains("walkintub"))
            {
                return "BATH_WALK_IN_TUB";
            }

            if (projectType.Contains("tubtoshower") ||
                projectType.Contains("tubconversion") ||
                projectType.Contains("showconversion"))
            {
                return "BATH_TUB_TO_SHOWER";
            }

            if (projectType.Contains("bathtub") ||
                projectType.Contains("showerinstall") ||
                projectType.Contains("showerreplace") ||
                projectType.Contains("tubinstall") ||
                projectType.Contains("tubreplace"))
            {
                return "BATH_TUB_SHOWER_INSTALL";
            }

            if (projectType.Contains("bathroomremodel") ||
                projectType.Contains("bathremodel") ||
                projectType.Contains("remodel") ||
                projectType.Contains("renovation"))
            {
                return "BATHROOM_REMODEL";
            }

            return null;
        }
        private static Dictionary<string, JsonElement> ParseAdditionalData(
            string? json)
        {
            var result =
                new Dictionary<string, JsonElement>(
                    StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(json))
            {
                return result;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(json);

                if (document.RootElement.ValueKind !=
                    JsonValueKind.Object)
                {
                    return result;
                }

                // Preserve root-level fields for historical/flat payloads.
                foreach (var property in
                    document.RootElement.EnumerateObject())
                {
                    if (!string.Equals(
                            property.Name,
                            "answers",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        result[property.Name] =
                            property.Value.Clone();
                    }
                }

                // Current Homeyy forms store project answers inside "answers".
                if (document.RootElement.TryGetProperty(
                        "answers",
                        out var answers) &&
                    answers.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in
                        answers.EnumerateObject())
                    {
                        result[property.Name] =
                            property.Value.Clone();
                    }
                }

                return result;
            }
            catch
            {
                return result;
            }
        }

        private static string? GetString(
            Dictionary<string, JsonElement> data,
            string key)
        {
            var item = data.FirstOrDefault(x =>
                string.Equals(
                    x.Key,
                    key,
                    StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(item.Key))
                return null;

            if (item.Value.ValueKind == JsonValueKind.String)
                return item.Value.GetString();

            return item.Value.ToString();
        }

        private static string NormalizeValue(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .Replace("/", string.Empty)
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
        }


        private async Task<ExternalLeadDeliveryResult>
            ExecutePingPostAsync(
                Lead lead,
                string taskId,
                CancellationToken cancellationToken)
        {
            var sourceId = NormalizeSourceId(
                _options.SourceId);

            var pingFields = BuildPingFields(
                lead,
                taskId,
                sourceId);

            var pingRequestPayload =
                SerializeFieldsForLogging(pingFields);

            using var pingResponse = await SendAsync(
                pingFields,
                cancellationToken);

            var pingResponseText =
                await pingResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            var pingHttpStatus =
                (int)pingResponse.StatusCode;

            var parsedPing = ParseResponse(
                pingResponseText);

            if (!pingResponse.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(
                        pingResponse.StatusCode),
                    httpStatusCode: pingHttpStatus,
                    requestPayload: pingRequestPayload,
                    responsePayload: pingResponseText,
                    errorMessage:
                        parsedPing.ErrorMessage ??
                        $"Networx ping returned HTTP " +
                        $"{pingHttpStatus}.");
            }

            if (!IsSuccessful(parsedPing))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: pingHttpStatus,
                    requestPayload: pingRequestPayload,
                    responsePayload: pingResponseText,
                    errorMessage:
                        parsedPing.ErrorMessage ??
                        $"Networx denied the ping. " +
                        $"StatusCode: {parsedPing.StatusCode}");
            }

            if (string.IsNullOrWhiteSpace(
                    parsedPing.Token))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: pingHttpStatus,
                    requestPayload: pingRequestPayload,
                    responsePayload: pingResponseText,
                    errorMessage:
                        "Networx ping succeeded but returned no token.");
            }

            var postFields = BuildPostFields(
                lead,
                taskId,
                sourceId,
                parsedPing.Token);

            var postRequestPayload =
                SerializeFieldsForLogging(postFields);

            using var postResponse = await SendAsync(
                postFields,
                cancellationToken);

            var postResponseText =
                await postResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            var postHttpStatus =
                (int)postResponse.StatusCode;

            var parsedPost = ParseResponse(
                postResponseText);

            var combinedRequest =
                $@"{{""ping"":{pingRequestPayload},""post"":{postRequestPayload}}}";

            var combinedResponse =
                $@"{{""ping"":{ToJsonString(pingResponseText)},""post"":{ToJsonString(postResponseText)}}}";

            if (!postResponse.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(
                        postResponse.StatusCode),
                    httpStatusCode: postHttpStatus,
                    requestPayload: combinedRequest,
                    responsePayload: combinedResponse,
                    errorMessage:
                        parsedPost.ErrorMessage ??
                        $"Networx post returned HTTP " +
                        $"{postHttpStatus}.");
            }

            if (!IsSuccessful(parsedPost))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: postHttpStatus,
                    requestPayload: combinedRequest,
                    responsePayload: combinedResponse,
                    errorMessage:
                        parsedPost.ErrorMessage ??
                        $"Networx denied the post. " +
                        $"StatusCode: {parsedPost.StatusCode}");
            }

            return ExternalLeadDeliveryResult.Success(
                httpStatusCode: postHttpStatus,
                externalReferenceId:
                    parsedPost.SuccessCode ??
                    parsedPing.Token,
                requestPayload: combinedRequest,
                responsePayload: combinedResponse);
        }

        private async Task<ExternalLeadDeliveryResult>
            ExecuteDirectPostAsync(
                Lead lead,
                string taskId,
                CancellationToken cancellationToken)
        {
            var sourceId = NormalizeSourceId(
                _options.SourceId);

            var fields = BuildPostFields(
                lead,
                taskId,
                sourceId,
                token: null);

            var requestPayload =
                SerializeFieldsForLogging(fields);

            using var response = await SendAsync(
                fields,
                cancellationToken);

            var responseText =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            var httpStatus =
                (int)response.StatusCode;

            var parsed = ParseResponse(
                responseText);

            if (!response.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(
                        response.StatusCode),
                    httpStatusCode: httpStatus,
                    requestPayload: requestPayload,
                    responsePayload: responseText,
                    errorMessage:
                        parsed.ErrorMessage ??
                        $"Networx direct post returned HTTP " +
                        $"{httpStatus}.");
            }

            if (!IsSuccessful(parsed))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: httpStatus,
                    requestPayload: requestPayload,
                    responsePayload: responseText,
                    errorMessage:
                        parsed.ErrorMessage ??
                        $"Networx denied the direct post. " +
                        $"StatusCode: {parsed.StatusCode}");
            }

            return ExternalLeadDeliveryResult.Success(
                httpStatusCode: httpStatus,
                externalReferenceId:
                    parsed.SuccessCode,
                requestPayload: requestPayload,
                responsePayload: responseText);
        }

        private Dictionary<string, string> BuildPingFields(
      Lead lead,
      string taskId,
      string sourceId)
        {
            return RemoveEmptyValues(
                new Dictionary<string, string?>
                {
                    ["nx_access_key"] =
                        _options.AccessKey.Trim(),

                    ["nx_userId"] =
                        _options.UserId.Trim(),

                    ["zipcode"] =
                        GetZipCode(lead),

                    ["task_id"] =
                        taskId.Trim(),

                    ["tcpa_compliance_text"] =
                        lead.TcpaComplianceText?.Trim()
                });
        }

        private Dictionary<string, string> BuildPostFields(
            Lead lead,
            string taskId,
            string sourceId,
            string? token)
        {
            var names = SplitName(
                lead.FullName);

            var comments =
                BuildComments(lead);

            return RemoveEmptyValues(
                new Dictionary<string, string?>
                {
                    ["nx_access_key"] =
                        _options.AccessKey,

                    ["nx_userId"] =
                        _options.UserId,

                    ["f_name"] =
                        names.FirstName,

                    ["l_name"] =
                        names.LastName,

                    ["zipcode"] =
                        GetZipCode(lead),

                    ["task_id"] =
                        taskId,

                    ["phone"] =
                        NormalizePhone(lead.Phone),

                    ["email"] =
                        lead.Email,

                    ["tcpa_compliance_text"] =
                        lead.TcpaComplianceText,

                    ["cert_url"] =
                        lead.TrustedFormCertificateUrl,

                    ["token"] =
                        token,

                    ["comments"] =
                        comments,

                    ["address"] =
                        lead.Address,

                    ["custom_id"] =
                        lead.Id.ToString(),

                    ["source_id"] =
                        sourceId,

                    ["is_business"] =
                        "0"
                });
        }

        private async Task<HttpResponseMessage> SendAsync(
            Dictionary<string, string> fields,
            CancellationToken cancellationToken)
        {
            using var content =
                new FormUrlEncodedContent(fields);

            return await _httpClient.PostAsync(
                _options.BaseUrl,
                content,
                cancellationToken);
        }

        private string GetZipCode(Lead lead)
        {
            // Networx explicitly requires ZIP 00001 for tests.
            if (_options.UseTestMode || lead.IsTest)
                return "00001";

            return lead.Postcode?.Trim() ??
                   string.Empty;
        }

        private static string? ValidateLead(Lead lead)
        {
            if (string.IsNullOrWhiteSpace(
                    lead.FullName))
            {
                return
                    "Full name is required for Networx.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Phone))
            {
                return
                    "Phone is required for Networx.";
            }

            if (NormalizePhone(lead.Phone)?.Length != 10)
            {
                return
                    "Networx requires a 10-digit phone number.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Email))
            {
                return
                    "Email is required for Networx.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.Postcode))
            {
                return
                    "Postcode is required for Networx.";
            }

            if (string.IsNullOrWhiteSpace(
                    lead.TcpaComplianceText))
            {
                return
                    "TCPA compliance text is required for Networx.";
            }

            /*
             * Networx requires cert_url on a post.
             * During test mode we allow the request through so the
             * Networx test response and transport can be verified.
             */
            if (!lead.IsTest &&
                string.IsNullOrWhiteSpace(
                    lead.TrustedFormCertificateUrl))
            {
                return
                    "TrustedForm certificate URL is required for Networx.";
            }

            return null;
        }

        private static NetworxResponse ParseResponse(
            string responseText)
        {
            if (string.IsNullOrWhiteSpace(
                    responseText))
            {
                return new NetworxResponse
                {
                    ErrorMessage =
                        "Networx returned an empty response."
                };
            }

            try
            {
                var document =
                    XDocument.Parse(responseText);

                var root =
                    document.Root;

                if (root == null)
                {
                    return new NetworxResponse
                    {
                        ErrorMessage =
                            "Networx returned XML without a root element."
                    };
                }

                return new NetworxResponse
                {
                    StatusCode =
                        GetElementValue(
                            root,
                            "statusCode"),

                    ErrorMessage =
                        GetElementValue(
                            root,
                            "errorMessage"),

                    Token =
                        GetElementValue(
                            root,
                            "token"),

                    Price =
                        GetElementValue(
                            root,
                            "price"),

                    SuccessCode =
                        GetElementValue(
                            root,
                            "successCode")
                };
            }
            catch (Exception)
            {
                return new NetworxResponse
                {
                    ErrorMessage =
                        "Networx returned invalid XML: " +
                        responseText
                };
            }
        }

        private static bool IsSuccessful(
            NetworxResponse response)
        {
            return string.Equals(
                       response.StatusCode,
                       "200",
                       StringComparison.OrdinalIgnoreCase) &&
                   string.IsNullOrWhiteSpace(
                       response.ErrorMessage);
        }

        private static string? GetElementValue(
            XElement root,
            string elementName)
        {
            return root
                .Descendants()
                .FirstOrDefault(x =>
                    string.Equals(
                        x.Name.LocalName,
                        elementName,
                        StringComparison.OrdinalIgnoreCase))
                ?.Value
                ?.Trim();
        }

        private static string BuildComments(
            Lead lead)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(
                    lead.CampaignName))
            {
                parts.Add(
                    $"Campaign: {lead.CampaignName}");
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.PageName))
            {
                parts.Add(
                    $"Page: {lead.PageName}");
            }

            if (!string.IsNullOrWhiteSpace(
                    lead.AdditionalDataJson))
            {
                parts.Add(
                    $"Project data: {lead.AdditionalDataJson}");
            }

            return string.Join(
                " | ",
                parts);
        }

        private static Dictionary<string, string>
            RemoveEmptyValues(
                Dictionary<string, string?> source)
        {
            return source
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.Value))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!);
        }

        private static string NormalizeSourceId(
            string? value)
        {
            var source =
                string.IsNullOrWhiteSpace(value)
                    ? "Homeyy"
                    : value.Trim();

            return source.Length <= 15
                ? source
                : source[..15];
        }

        private static (
            string FirstName,
            string LastName) SplitName(
                string? fullName)
        {
            if (string.IsNullOrWhiteSpace(
                    fullName))
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            var parts = fullName
                .Trim()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return (
                    parts[0],
                    "Unknown");
            }

            return (
                parts[0],
                string.Join(
                    " ",
                    parts.Skip(1)));
        }

        private static string? NormalizePhone(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(
                    phone))
            {
                return null;
            }

            var digits = new string(
                phone
                    .Where(char.IsDigit)
                    .ToArray());

            if (digits.Length > 10)
            {
                digits = digits[^10..];
            }

            return digits;
        }

        private static bool IsRetryableStatus(
            HttpStatusCode statusCode)
        {
            return
                statusCode ==
                    HttpStatusCode.RequestTimeout ||
                statusCode ==
                    HttpStatusCode.TooManyRequests ||
                (int)statusCode >= 500;
        }

        private static string
            SerializeFieldsForLogging(
                Dictionary<string, string> fields)
        {
            var safeFields =
                new Dictionary<string, string>(
                    fields,
                    StringComparer.OrdinalIgnoreCase);

            if (safeFields.ContainsKey(
                    "nx_access_key"))
            {
                safeFields["nx_access_key"] =
                    "***REDACTED***";
            }

            return System.Text.Json.JsonSerializer
                .Serialize(safeFields);
        }

        private static string ToJsonString(
            string value)
        {
            return System.Text.Json.JsonSerializer
                .Serialize(value);
        }

        private static ExternalLeadDeliveryResult Failed(
            string message)
        {
            return ExternalLeadDeliveryResult.Failed(
                isRetryable: false,
                httpStatusCode: null,
                requestPayload: null,
                responsePayload: null,
                errorMessage: message);
        }

        private sealed class NetworxResponse
        {
            public string? StatusCode { get; set; }

            public string? ErrorMessage { get; set; }

            public string? Token { get; set; }

            public string? Price { get; set; }

            public string? SuccessCode { get; set; }
        }
    }
}

 