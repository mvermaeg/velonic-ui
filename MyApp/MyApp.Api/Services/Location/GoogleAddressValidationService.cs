using System.Text;
using System.Text.Json;
using MyApp.Api.DTOs.Location;

namespace MyApp.Api.Services.Location
{
    public class GoogleAddressValidationService : IAddressValidationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GoogleAddressValidationService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<VerifyAddressPostcodeResponse> VerifyAsync(
            VerifyAddressPostcodeRequest model)
        {
            var apiKey = _configuration["GoogleMaps:AddressValidationApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    NeedsReview = false,
                    Status = "ConfigMissing",
                    Message = "Google Address Validation API key is missing."
                };
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    NeedsReview = false,
                    Status = "InvalidRequest",
                    Message = "Address is required."
                };
            }

            if (string.IsNullOrWhiteSpace(model.Postcode))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    NeedsReview = false,
                    Status = "InvalidRequest",
                    Message = "Postcode is required."
                };
            }

            var fullAddress = $"{model.Address}, {model.Postcode}";

            var requestBody = new
            {
                address = new
                {
                    regionCode = string.IsNullOrWhiteSpace(model.CountryCode)
                        ? "IN"
                        : model.CountryCode.Trim().ToUpperInvariant(),
                    addressLines = new[] { fullAddress }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://addressvalidation.googleapis.com/v1:validateAddress?key={apiKey}");

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    NeedsReview = false,
                    Status = "ProviderError",
                    Message = $"Google error {(int)response.StatusCode}: {responseJson}"
                };
            }

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("result", out var result))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    NeedsReview = false,
                    Status = "ProviderError",
                    Message = "Google response does not contain result."
                };
            }

            var verdict = result.TryGetProperty("verdict", out var verdictElement)
                ? verdictElement
                : default;

            var possibleNextAction =
                verdict.ValueKind != JsonValueKind.Undefined &&
                verdict.TryGetProperty("possibleNextAction", out var actionElement)
                    ? actionElement.GetString()
                    : "";

            string? formattedAddress = null;
            string? postalCode = null;
            string? city = null;
            string? state = null;
            string? country = null;

            if (result.TryGetProperty("address", out var addressElement))
            {
                if (addressElement.TryGetProperty("formattedAddress", out var formattedElement))
                    formattedAddress = formattedElement.GetString();

                if (addressElement.TryGetProperty("postalAddress", out var postalAddressElement))
                {
                    if (postalAddressElement.TryGetProperty("postalCode", out var postalCodeElement))
                        postalCode = postalCodeElement.GetString();

                    if (postalAddressElement.TryGetProperty("locality", out var cityElement))
                        city = cityElement.GetString();

                    if (postalAddressElement.TryGetProperty("administrativeArea", out var stateElement))
                        state = stateElement.GetString();

                    if (postalAddressElement.TryGetProperty("regionCode", out var countryElement))
                        country = countryElement.GetString();
                }
            }

            var inputPostcode = Normalize(model.Postcode);
            var returnedPostcode = Normalize(postalCode);

            var postalCodeMatches =
                !string.IsNullOrWhiteSpace(inputPostcode) &&
                !string.IsNullOrWhiteSpace(returnedPostcode) &&
                inputPostcode == returnedPostcode;

            var isAccept = string.Equals(
                possibleNextAction,
                "ACCEPT",
                StringComparison.OrdinalIgnoreCase);

            if (postalCodeMatches)
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = true,
                    NeedsReview = !isAccept,
                    Status = isAccept ? "Valid" : "Review",
                    Message = isAccept
                        ? "Address and postal code match."
                        : "Postal code matches, but address may need correction.",
                    FormattedAddress = formattedAddress,
                    PostalCode = postalCode,
                    City = city,
                    State = state,
                    Country = country,
                    PossibleNextAction = possibleNextAction
                };
            }

            return new VerifyAddressPostcodeResponse
            {
                IsValid = false,
                NeedsReview = false,
                Status = "Mismatch",
                Message = "Address and postal code do not match.",
                FormattedAddress = formattedAddress,
                PostalCode = postalCode,
                City = city,
                State = state,
                Country = country,
                PossibleNextAction = possibleNextAction
            };
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            var cleaned = value.Trim().ToUpperInvariant().Replace(" ", "");

            if (cleaned.Contains("-"))
                cleaned = cleaned.Split('-')[0];

            return cleaned;
        }
    }
}