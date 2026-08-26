using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyApp.Api.DTOs.Ringba;

namespace MyApp.Api.Services.Ringba
{
    public class RingbaService
    {
        private readonly HttpClient _httpClient;
        private readonly RingbaOptions _options;
        private readonly ILogger<RingbaService> _logger;

        public RingbaService(
            HttpClient httpClient,
            IOptions<RingbaOptions> options,
            ILogger<RingbaService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<RingbaBidResponseDto> RequestBidAsync(
            RingbaBidRequestDto model,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error = "Ringba integration is disabled."
                };
            }

            var validationError = Validate(model);

            if (validationError != null)
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error = validationError
                };
            }

            var vertical = NormalizeVertical(model.Vertical);

            if (vertical == null)
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error =
                        $"Unsupported Ringba vertical '{model.Vertical}'. " +
                        "Supported values are Roofing, Windows, HVAC and Plumbing."
                };
            }

            if (!_options.RtbIds.TryGetValue(
                    vertical,
                    out var rtbId) ||
                string.IsNullOrWhiteSpace(rtbId))
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error =
                        $"Ringba RTB ID is not configured for vertical '{vertical}'."
                };
            }

            var cid = NormalizeUsPhoneToE164Digits(
                model.CallerPhone);

            if (cid == null)
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error =
                        "CallerPhone must be a valid United States phone number."
                };
            }

            var baseUrl = _options.BaseUrl.TrimEnd('/');

            var requestUrl =
                $"{baseUrl}/{rtbId}.json";

            var payload = new Dictionary<string, object?>
            {
                ["CID"] = cid,
                ["exposeCallerId"] = "yes",

                // Ringba requires both keys.
                ["zipcode"] = model.ZipCode.Trim(),
                ["Zipcode"] = model.ZipCode.Trim(),

                ["SubID"] = NullIfWhiteSpace(model.SubId),
                ["FirstName"] = NullIfWhiteSpace(model.FirstName),
                ["LastName"] = NullIfWhiteSpace(model.LastName),
                ["Email"] = NullIfWhiteSpace(model.Email),
                ["Address"] = NullIfWhiteSpace(model.Address),
                ["City"] = NullIfWhiteSpace(model.City),
                ["State"] = NullIfWhiteSpace(model.State)
            };

            var cleanedPayload = payload
                .Where(x => x.Value != null)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value);

            try
            {
                using var response =
                    await _httpClient.PostAsJsonAsync(
                        requestUrl,
                        cleanedPayload,
                        cancellationToken);

                var rawResponse =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Ringba request failed. Vertical: {Vertical}, " +
                        "HTTP: {HttpStatusCode}, Response: {Response}",
                        vertical,
                        (int)response.StatusCode,
                        rawResponse);

                    return new RingbaBidResponseDto
                    {
                        Success = false,
                        HttpStatusCode = (int)response.StatusCode,
                        RawResponse = rawResponse,
                        Error =
                            $"Ringba returned HTTP {(int)response.StatusCode}."
                    };
                }

                return ParseResponse(
                    rawResponse,
                    (int)response.StatusCode);
            }
            catch (TaskCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    "Ringba request timed out for vertical {Vertical}.",
                    vertical);

                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error = "Ringba request timed out."
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Ringba HTTP request failed for vertical {Vertical}.",
                    vertical);

                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected Ringba error for vertical {Vertical}.",
                    vertical);

                return new RingbaBidResponseDto
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private static RingbaBidResponseDto ParseResponse(
            string rawResponse,
            int httpStatusCode)
        {
            if (string.IsNullOrWhiteSpace(rawResponse))
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    HttpStatusCode = httpStatusCode,
                    RawResponse = rawResponse,
                    Error = "Ringba returned an empty response."
                };
            }

            try
            {
                using var document =
                    JsonDocument.Parse(rawResponse);

                var root = document.RootElement;

                var bidId =
                    GetString(root, "bidId");

                var phoneNumber =
                    GetString(root, "phoneNumber");

                var phoneNumberNoPlus =
                    GetString(root, "phoneNumberNoPlus");

                var sipAddress =
                    GetString(root, "sipAddress");

                var bidAmount =
                    GetDecimal(root, "bidAmount");

                var expireInSeconds =
                    GetInt32(root, "expireInSeconds");

                var bidExpireDateTime =
                    GetString(root, "bidExpireDT");

                var bidExpireEpoch =
                    GetInt64(root, "bidExpireEpoch");

                var bidTermsJson =
                    GetRawJson(root, "bidTerms");

                var warningsJson =
                    GetRawJson(root, "warnings");

                var hasUsableDestination =
                    !string.IsNullOrWhiteSpace(phoneNumber) ||
                    !string.IsNullOrWhiteSpace(sipAddress);

                var success =
                    !string.IsNullOrWhiteSpace(bidId) &&
                    hasUsableDestination &&
                    expireInSeconds.HasValue &&
                    expireInSeconds.Value > 0;

                return new RingbaBidResponseDto
                {
                    Success = success,
                    BidId = bidId,
                    BidAmount = bidAmount,
                    ExpireInSeconds = expireInSeconds,
                    BidExpireDateTime = bidExpireDateTime,
                    BidExpireEpoch = bidExpireEpoch,
                    PhoneNumber = phoneNumber,
                    PhoneNumberNoPlus = phoneNumberNoPlus,
                    SipAddress = sipAddress,
                    BidTermsJson = bidTermsJson,
                    WarningsJson = warningsJson,
                    HttpStatusCode = httpStatusCode,
                    RawResponse = rawResponse,
                    Error = success
                        ? null
                        : ExtractError(root)
                            ?? "Ringba returned no usable bid."
                };
            }
            catch (JsonException ex)
            {
                return new RingbaBidResponseDto
                {
                    Success = false,
                    HttpStatusCode = httpStatusCode,
                    RawResponse = rawResponse,
                    Error =
                        $"Ringba returned invalid JSON: {ex.Message}"
                };
            }
        }

        private static string? Validate(
            RingbaBidRequestDto model)
        {
            if (model == null)
                return "Request body is required.";

            if (string.IsNullOrWhiteSpace(model.Vertical))
                return "Vertical is required.";

            if (string.IsNullOrWhiteSpace(model.CallerPhone))
                return "CallerPhone is required.";

            if (string.IsNullOrWhiteSpace(model.ZipCode))
                return "ZipCode is required.";

            var zipDigits =
                new string(
                    model.ZipCode
                        .Where(char.IsDigit)
                        .ToArray());

            if (zipDigits.Length != 5)
                return "ZipCode must be a valid 5-digit US ZIP code.";

            model.ZipCode = zipDigits;

            return null;
        }

        private static string? NormalizeVertical(
            string? vertical)
        {
            if (string.IsNullOrWhiteSpace(vertical))
                return null;

            var normalized =
                vertical.Trim()
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty)
                    .ToLowerInvariant();

            return normalized switch
            {
                "roofing" or "roof" => "Roofing",
                "windows" or "window" => "Windows",
                "hvac" or "heating" or "cooling" => "HVAC",
                "plumbing" or "plumber" => "Plumbing",
                _ => null
            };
        }

        private static string? NormalizeUsPhoneToE164Digits(
            string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            var digits =
                new string(
                    phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 10)
                return "1" + digits;

            if (digits.Length == 11 &&
                digits.StartsWith("1"))
            {
                return digits;
            }

            return null;
        }

        private static object? NullIfWhiteSpace(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? GetString(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.String)
                return property.GetString();

            return property.ValueKind is
                JsonValueKind.Null or
                JsonValueKind.Undefined
                    ? null
                    : property.ToString();
        }

        private static decimal? GetDecimal(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number &&
                property.TryGetDecimal(out var value))
            {
                return value;
            }

            return decimal.TryParse(
                property.ToString(),
                out value)
                    ? value
                    : null;
        }

        private static int? GetInt32(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt32(out var value))
            {
                return value;
            }

            return int.TryParse(
                property.ToString(),
                out value)
                    ? value
                    : null;
        }

        private static long? GetInt64(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number &&
                property.TryGetInt64(out var value))
            {
                return value;
            }

            return long.TryParse(
                property.ToString(),
                out value)
                    ? value
                    : null;
        }

        private static string? GetRawJson(
            JsonElement element,
            string propertyName)
        {
            return element.TryGetProperty(
                    propertyName,
                    out var property)
                ? property.GetRawText()
                : null;
        }

        private static string? ExtractError(
            JsonElement root)
        {
            var possibleProperties = new[]
            {
                "error",
                "errors",
                "message",
                "errorMessage",
                "reason"
            };

            foreach (var propertyName in possibleProperties)
            {
                if (!root.TryGetProperty(
                        propertyName,
                        out var property))
                {
                    continue;
                }

                if (property.ValueKind == JsonValueKind.String)
                    return property.GetString();

                return property.GetRawText();
            }

            return null;
        }
    }
}