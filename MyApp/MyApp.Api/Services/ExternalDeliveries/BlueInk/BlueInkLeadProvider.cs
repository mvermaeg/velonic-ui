using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries.BlueInk
{
    public class BlueInkLeadProvider : IExternalLeadProvider
    {
        public const string ProviderCode = "BLUEINK";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = null,
            WriteIndented = false
        };

        private readonly HttpClient _httpClient;
        private readonly BlueInkOptions _options;
        private readonly ILogger<BlueInkLeadProvider> _logger;

        public BlueInkLeadProvider(
            HttpClient httpClient,
            IOptions<BlueInkOptions> options,
            ILogger<BlueInkLeadProvider> logger)
        {
            _httpClient = httpClient;
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
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "Blue Ink integration is disabled.");
            }

            var verticalCode = NormalizeVertical(delivery.VerticalCode);

            if (string.IsNullOrWhiteSpace(verticalCode))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "Blue Ink vertical is missing or unsupported.");
            }

            if (!_options.Verticals.TryGetValue(
                    verticalCode,
                    out var verticalOptions))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        $"Blue Ink configuration was not found for vertical '{verticalCode}'.");
            }

            if (!verticalOptions.Enabled)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        $"Blue Ink vertical '{verticalCode}' is disabled.");
            }

            if (string.IsNullOrWhiteSpace(verticalOptions.Token))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage:
                        $"Blue Ink token is missing for vertical '{verticalCode}'.");
            }

            var validationError = ValidateLead(lead);

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
                if (_options.UsePingPost)
                {
                    return await ExecutePingPostAsync(
                        lead,
                        verticalCode,
                        verticalOptions,
                        cancellationToken);
                }

                return await ExecuteDirectPostAsync(
                    lead,
                    verticalCode,
                    verticalOptions,
                    cancellationToken);
            }
            catch (TaskCanceledException ex)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    ex,
                    "Blue Ink request timed out for LeadId {LeadId}.",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: "Blue Ink request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Blue Ink HTTP request failed for LeadId {LeadId}.",
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
                    "Unexpected Blue Ink error for LeadId {LeadId}.",
                    lead.Id);

                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: ex.Message);
            }
        }

        private async Task<ExternalLeadDeliveryResult> ExecutePingPostAsync(
            Lead lead,
            string verticalCode,
            BlueInkVerticalOptions verticalOptions,
            CancellationToken cancellationToken)
        {
            var pingUrl = _options.UseTestEndpoints
                ? verticalOptions.TestPingUrl
                : verticalOptions.ProductionPingUrl;

            var postUrl = _options.UseTestEndpoints
                ? verticalOptions.TestPostUrl
                : verticalOptions.ProductionPostUrl;

            var pingPayload = BuildPayload(
                lead,
                verticalCode,
                authCode: null,
                includeFullContact: false);

            var pingJson = JsonSerializer.Serialize(
                pingPayload,
                JsonOptions);

            var pingResponse = await SendAsync(
                pingUrl,
                verticalOptions.Token,
                pingJson,
                cancellationToken);

            var pingResponseText =
                await pingResponse.Content.ReadAsStringAsync(cancellationToken);

            var pingStatusCode = (int)pingResponse.StatusCode;

            if (!pingResponse.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(pingResponse.StatusCode),
                    httpStatusCode: pingStatusCode,
                    requestPayload: pingJson,
                    responsePayload: pingResponseText,
                    errorMessage:
                        $"Blue Ink ping returned HTTP {pingStatusCode}.");
            }

            var pingResult = ParseResponse(pingResponseText);

            if (!string.Equals(
                    pingResult.Status,
                    "success",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: pingStatusCode,
                    requestPayload: pingJson,
                    responsePayload: pingResponseText,
                    errorMessage:
                        pingResult.Errors ??
                        "Blue Ink denied the ping request.");
            }

            if (string.IsNullOrWhiteSpace(pingResult.AuthCode))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: pingStatusCode,
                    requestPayload: pingJson,
                    responsePayload: pingResponseText,
                    errorMessage:
                        "Blue Ink ping succeeded but returned no auth_code.");
            }

            var postPayload = BuildPayload(
                lead,
                verticalCode,
                pingResult.AuthCode,
                includeFullContact: true);

            var postJson = JsonSerializer.Serialize(
                postPayload,
                JsonOptions);

            var postResponse = await SendAsync(
                postUrl,
                verticalOptions.Token,
                postJson,
                cancellationToken);

            var postResponseText =
                await postResponse.Content.ReadAsStringAsync(cancellationToken);

            var postStatusCode = (int)postResponse.StatusCode;

            var combinedRequest = JsonSerializer.Serialize(new
            {
                ping = JsonSerializer.Deserialize<object>(pingJson),
                post = JsonSerializer.Deserialize<object>(postJson)
            });

            var combinedResponse = JsonSerializer.Serialize(new
            {
                ping = TryDeserializeResponse(pingResponseText),
                post = TryDeserializeResponse(postResponseText)
            });

            if (!postResponse.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(postResponse.StatusCode),
                    httpStatusCode: postStatusCode,
                    requestPayload: combinedRequest,
                    responsePayload: combinedResponse,
                    errorMessage:
                        $"Blue Ink post returned HTTP {postStatusCode}.");
            }

            var postResult = ParseResponse(postResponseText);

            if (!string.Equals(
                    postResult.Status,
                    "success",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: postStatusCode,
                    requestPayload: combinedRequest,
                    responsePayload: combinedResponse,
                    errorMessage:
                        postResult.Errors ??
                        "Blue Ink denied the post request.");
            }

            return ExternalLeadDeliveryResult.Success(
                httpStatusCode: postStatusCode,
                externalReferenceId:
                    postResult.ConfirmationId ?? pingResult.AuthCode,
                requestPayload: combinedRequest,
                responsePayload: combinedResponse);
        }

        private async Task<ExternalLeadDeliveryResult> ExecuteDirectPostAsync(
            Lead lead,
            string verticalCode,
            BlueInkVerticalOptions verticalOptions,
            CancellationToken cancellationToken)
        {
            var postUrl = _options.UseTestEndpoints
                ? verticalOptions.TestPostUrl
                : verticalOptions.ProductionPostUrl;

            var payload = BuildPayload(
                lead,
                verticalCode,
                authCode: null,
                includeFullContact: true);

            var requestJson = JsonSerializer.Serialize(
                payload,
                JsonOptions);

            var response = await SendAsync(
                postUrl,
                verticalOptions.Token,
                requestJson,
                cancellationToken);

            var responseText =
                await response.Content.ReadAsStringAsync(cancellationToken);

            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: IsRetryableStatus(response.StatusCode),
                    httpStatusCode: statusCode,
                    requestPayload: requestJson,
                    responsePayload: responseText,
                    errorMessage:
                        $"Blue Ink direct post returned HTTP {statusCode}.");
            }

            var parsedResponse = ParseResponse(responseText);

            if (!string.Equals(
                    parsedResponse.Status,
                    "success",
                    StringComparison.OrdinalIgnoreCase))
            {
                return ExternalLeadDeliveryResult.Failed(
                    isRetryable: false,
                    httpStatusCode: statusCode,
                    requestPayload: requestJson,
                    responsePayload: responseText,
                    errorMessage:
                        parsedResponse.Errors ??
                        "Blue Ink denied the direct post.");
            }

            return ExternalLeadDeliveryResult.Success(
                httpStatusCode: statusCode,
                externalReferenceId: parsedResponse.ConfirmationId,
                requestPayload: requestJson,
                responsePayload: responseText);
        }

        private async Task<HttpResponseMessage> SendAsync(
            string url,
            string token,
            string json,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                url);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            request.Headers.TryAddWithoutValidation(
                "Authorization",
                $"Token {token.Trim()}");

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            return await _httpClient.SendAsync(
                request,
                cancellationToken);
        }

        private static Dictionary<string, object?> BuildPayload(
            Lead lead,
            string verticalCode,
            string? authCode,
            bool includeFullContact)
        {
            var additionalData = ParseAdditionalData(
                lead.AdditionalDataJson);

            var names = SplitName(lead.FullName);

            var data = BuildVerticalData(
                verticalCode,
                lead,
                additionalData);

            var meta = new Dictionary<string, object?>
            {
                ["s2"] = lead.AffiliateSubId,
                ["offer_id"] = lead.CampaignName,
                ["source_id"] =
                    lead.SourceReference ??
                    lead.Source ??
                    "Homeyy",
                ["user_agent"] = lead.UserAgent,
                ["lead_id_code"] = lead.JornayaLeadId,
                ["tcpa_compliant"] = lead.IsTcpaCompliant,
                ["landing_page_url"] = lead.LandingPageUrl,
                ["tcpa_consent_text"] = lead.TcpaComplianceText,
                ["originally_created"] =
                    lead.CreatedAt.ToUniversalTime()
                        .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                ["trusted_form_cert_url"] =
                    lead.TrustedFormCertificateUrl
            };

            var contact = new Dictionary<string, object?>
            {
                ["zip_code"] = lead.Postcode,
                ["ip_address"] = lead.IpAddress
            };

            if (includeFullContact)
            {
                contact["city"] = lead.City;
                contact["email"] = lead.Email;
                contact["phone"] = NormalizePhone(lead.Phone);
                contact["state"] = lead.State;
                contact["address"] = lead.Address;
                contact["first_name"] = names.FirstName;
                contact["last_name"] = names.LastName;
                contact["phone_last_four"] =
                    GetPhoneLastFour(lead.Phone);
            }

            var payload = new Dictionary<string, object?>
            {
                ["data"] = data,
                ["meta"] = RemoveNullValues(meta),
                ["contact"] = RemoveNullValues(contact)
            };

            if (!string.IsNullOrWhiteSpace(authCode))
            {
                payload["auth_code"] = authCode;
            }

            return payload;
        }

        private static Dictionary<string, object?> BuildVerticalData(
    string verticalCode,
    Lead lead,
    Dictionary<string, JsonElement> additionalData)
        {
            var data = new Dictionary<string, object?>
            {
                ["own_property"] =
                    lead.OwnsProperty ??
                    GetBoolean(additionalData, "ownProperty"),

                ["credit_rating"] =
                    GetString(additionalData, "creditRating") ??
                    "Excellent",

                ["best_call_time"] =
                    GetString(additionalData, "bestCallTime") ??
                    "Anytime",

                ["purchase_time_frame"] =
                    GetString(additionalData, "purchaseTimeFrame") ??
                    "Immediately"
            };

            switch (verticalCode)
            {
                case "Roofing":
                    data["roof"] = RemoveNullValues(
                        new Dictionary<string, object?>
                        {
                            ["project_type"] =
                                GetString(additionalData, "projectType"),

                            ["roofing_type"] =
                                GetString(additionalData, "roofingType")
                        });
                    break;

                case "Windows":
                    data["windows"] = RemoveNullValues(
                        new Dictionary<string, object?>
                        {
                            ["material"] =
                                GetString(additionalData, "material") ??
                                GetString(additionalData, "windowMaterial"),

                            ["num_windows"] =
                                GetInteger(additionalData, "numWindows") ??
                                GetInteger(additionalData, "windowCount") ??
                                GetInteger(additionalData, "windowQuantity"),

                            ["project_type"] =
                                GetString(additionalData, "projectType")
                        });
                    break;

                case "Bathroom":
                    data["bathroom"] = RemoveNullValues(
                        new Dictionary<string, object?>
                        {
                            ["project_type"] =
                                GetString(additionalData, "projectType")
                        });
                    break;

                case "HVAC":
                    data["hvac"] = RemoveNullValues(
                        new Dictionary<string, object?>
                        {
                            ["air_type"] =
                                GetString(additionalData, "airType"),

                            ["system_type"] =
                                GetString(additionalData, "systemType"),

                            ["project_type"] =
                                GetString(additionalData, "projectType")
                        });
                    break;
            }

            return RemoveNullValues(data);
        }



        //    private static Dictionary<string, object?> BuildVerticalData(
        //string verticalCode,
        //Lead lead,
        //Dictionary<string, JsonElement> additionalData)
        //    {
        //        var data = new Dictionary<string, object?>
        //        {
        //            ["own_property"] =
        //                lead.OwnsProperty ??
        //                GetBoolean(additionalData, "ownProperty"),

        //            ["credit_rating"] =
        //                GetString(additionalData, "creditRating"),

        //            ["best_call_time"] =
        //                GetString(additionalData, "bestCallTime"),

        //            ["purchase_time_frame"] =
        //                GetString(additionalData, "purchaseTimeFrame")
        //        };

        //        switch (verticalCode)
        //        {
        //            case "Roofing":
        //                data["roof"] = RemoveNullValues(
        //                    new Dictionary<string, object?>
        //                    {
        //                        ["project_type"] =
        //                            GetString(additionalData, "projectType"),

        //                        ["roofing_type"] =
        //                            GetString(additionalData, "roofingType")
        //                    });
        //                break;

        //            case "Windows":
        //                data["windows"] = RemoveNullValues(
        //                    new Dictionary<string, object?>
        //                    {
        //                        ["material"] =
        //                            GetString(additionalData, "material") ??
        //                            GetString(additionalData, "windowMaterial"),

        //                        ["num_windows"] =
        //                            GetInteger(additionalData, "numWindows") ??
        //                            GetInteger(additionalData, "windowCount") ??
        //                            GetInteger(additionalData, "windowQuantity"),

        //                        ["project_type"] =
        //                            GetString(additionalData, "projectType")
        //                    });
        //                break;

        //            case "Bathroom":
        //                data["bathroom"] = RemoveNullValues(
        //                    new Dictionary<string, object?>
        //                    {
        //                        ["project_type"] =
        //                            GetString(additionalData, "projectType")
        //                    });
        //                break;

        //            case "HVAC":
        //                data["hvac"] = RemoveNullValues(
        //                    new Dictionary<string, object?>
        //                    {
        //                        ["air_type"] =
        //                            GetString(additionalData, "airType"),

        //                        ["system_type"] =
        //                            GetString(additionalData, "systemType"),

        //                        ["project_type"] =
        //                            GetString(additionalData, "projectType")
        //                    });
        //                break;
        //        }

        //        return RemoveNullValues(data);
        //    }

        private static string? ValidateLead(Lead lead)
        {
            if (string.IsNullOrWhiteSpace(lead.Postcode))
                return "Postcode is required for Blue Ink.";

            if (string.IsNullOrWhiteSpace(lead.IpAddress))
                return "IP address is required for Blue Ink.";

            if (!lead.OwnsProperty.HasValue)
                return "Property ownership is required for Blue Ink.";

            if (!lead.IsTest &&
     string.IsNullOrWhiteSpace(lead.TrustedFormCertificateUrl) &&
     string.IsNullOrWhiteSpace(lead.JornayaLeadId))
            {
                return
                    "Either TrustedForm certificate URL or Jornaya Lead ID is required for Blue Ink.";
            }

            return null;
        }

        private static string? NormalizeVertical(string? vertical)
        {
            if (string.IsNullOrWhiteSpace(vertical))
                return null;

            var value = vertical
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .ToLowerInvariant();

            return value switch
            {
                "roof" or "roofing" => "Roofing",

                "window" or "windows" => "Windows",

                "bathroom" or
                "bathroomremodeling" or
                "bathroomremodel" => "Bathroom",

                "hvac" or
                "heatingandcooling" => "HVAC",

                _ => null
            };
        }

        private static Dictionary<string, JsonElement> ParseAdditionalData(
            string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, JsonElement>(
                    StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                return JsonSerializer.Deserialize<
                    Dictionary<string, JsonElement>>(json)
                    ?? new Dictionary<string, JsonElement>(
                        StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, JsonElement>(
                    StringComparer.OrdinalIgnoreCase);
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

        private static bool? GetBoolean(
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

            if (item.Value.ValueKind == JsonValueKind.True)
                return true;

            if (item.Value.ValueKind == JsonValueKind.False)
                return false;

            if (bool.TryParse(item.Value.ToString(), out var value))
                return value;

            return null;
        }




        private static int? GetInteger(
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

            if (item.Value.ValueKind == JsonValueKind.Number &&
                item.Value.TryGetInt32(out var numericValue))
            {
                return numericValue;
            }

            var rawValue = item.Value.ValueKind == JsonValueKind.String
                ? item.Value.GetString()
                : item.Value.ToString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            if (int.TryParse(rawValue, out numericValue))
                return numericValue;

            var firstNumber = new string(
                rawValue
                    .SkipWhile(c => !char.IsDigit(c))
                    .TakeWhile(char.IsDigit)
                    .ToArray());

            return int.TryParse(firstNumber, out numericValue)
                ? numericValue
                : null;
        }


        private static Dictionary<string, object?> RemoveNullValues(
            Dictionary<string, object?> source)
        {
            return source
                .Where(x =>
                    x.Value != null &&
                    (!(x.Value is string value) ||
                     !string.IsNullOrWhiteSpace(value)))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private static (string FirstName, string LastName) SplitName(
            string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (string.Empty, string.Empty);

            var parts = fullName
                .Trim()
                .Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return (parts[0], string.Empty);

            return (
                parts[0],
                string.Join(" ", parts.Skip(1)));
        }

        private static string? NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var digits = new string(
                phone.Where(char.IsDigit).ToArray());

            if (digits.Length > 10)
                digits = digits[^10..];

            return digits;
        }

        private static string? GetPhoneLastFour(string? phone)
        {
            var normalized = NormalizePhone(phone);

            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            return normalized.Length <= 4
                ? normalized
                : normalized[^4..];
        }

        private static bool IsRetryableStatus(
            HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout ||
                   statusCode == HttpStatusCode.TooManyRequests ||
                   (int)statusCode >= 500;
        }

        private static BlueInkResponse ParseResponse(
            string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText))
            {
                return new BlueInkResponse
                {
                    Errors = "Blue Ink returned an empty response."
                };
            }

            try
            {
                using var document = JsonDocument.Parse(responseText);
                var root = document.RootElement;

                return new BlueInkResponse
                {
                    Status = GetJsonString(root, "status"),
                    AuthCode =
                        GetJsonString(root, "auth_code"),
                    ConfirmationId =
                        GetJsonString(root, "confirmation_id"),
                    Price = GetJsonString(root, "price"),
                    BuyerName =
                        GetJsonString(root, "buyer_name"),
                    Errors = GetJsonValueAsString(
                        root,
                        "errors")
                };
            }
            catch (JsonException)
            {
                return new BlueInkResponse
                {
                    Errors =
                        "Blue Ink returned invalid JSON: " +
                        responseText
                };
            }
        }

        private static object TryDeserializeResponse(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<object>(value)
                       ?? value;
            }
            catch
            {
                return value;
            }
        }

        private static string? GetJsonString(
            JsonElement root,
            string propertyName)
        {
            if (!root.TryGetProperty(
                    propertyName,
                    out var value))
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : value.ToString();
        }

        private static string? GetJsonValueAsString(
            JsonElement root,
            string propertyName)
        {
            if (!root.TryGetProperty(
                    propertyName,
                    out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Null => null,
                _ => value.GetRawText()
            };
        }

        private sealed class BlueInkResponse
        {
            public string? Status { get; set; }

            public string? AuthCode { get; set; }

            public string? ConfirmationId { get; set; }

            public string? Price { get; set; }

            public string? BuyerName { get; set; }

            public string? Errors { get; set; }
        }
    }
}