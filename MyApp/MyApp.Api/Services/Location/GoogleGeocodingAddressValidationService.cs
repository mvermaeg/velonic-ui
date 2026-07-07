using System.Text.Json;
using MyApp.Api.DTOs.Location;

namespace MyApp.Api.Services.Location
{
    public class GoogleGeocodingAddressValidationService : IAddressValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GoogleGeocodingAddressValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<VerifyAddressPostcodeResponse> VerifyAsync(VerifyAddressPostcodeRequest model)
        {
            var address = model.Address?.Trim();
            var postcode = model.Postcode?.Trim();
            var countryCode = string.IsNullOrWhiteSpace(model.CountryCode) ? "IN" : model.CountryCode.Trim();

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(postcode))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "Mismatch",
                    Message = "Address and postcode are required."
                };
            }

            var apiKey =
                _configuration["GoogleMaps:AddressValidationApiKey"] ??
                _configuration["GoogleMaps:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "ProviderError",
                    Message = "Google Maps API key is missing."
                };
            }

            var query = $"{address}, {postcode}";
            var url =
                "https://maps.googleapis.com/maps/api/geocode/json" +
                $"?address={Uri.EscapeDataString(query)}" +
                $"&components=country:{Uri.EscapeDataString(countryCode)}" +
                $"&key={Uri.EscapeDataString(apiKey)}";

            try
            {
                var json = await _httpClient.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var status = root.GetProperty("status").GetString();

                if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    return new VerifyAddressPostcodeResponse
                    {
                        IsValid = false,
                        Status = "ProviderError",
                        Message = $"Google Geocoding error: {status}"
                    };
                }

                var first = root.GetProperty("results")[0];

                string? foundPostalCode = null;
                string? city = null;
                string? state = null;
                string? country = null;

                foreach (var component in first.GetProperty("address_components").EnumerateArray())
                {
                    var longName = component.GetProperty("long_name").GetString();
                    var shortName = component.GetProperty("short_name").GetString();

                    var types = component.GetProperty("types")
                        .EnumerateArray()
                        .Select(x => x.GetString())
                        .ToList();

                    if (types.Contains("postal_code"))
                        foundPostalCode = longName;

                    if (types.Contains("locality") || types.Contains("postal_town"))
                        city = longName;

                    if (types.Contains("administrative_area_level_2") && string.IsNullOrWhiteSpace(city))
                        city = longName;

                    if (types.Contains("administrative_area_level_1"))
                        state = longName;

                    if (types.Contains("country"))
                        country = longName;
                }

                var normalizedInputPostcode = NormalizePostcode(postcode);
                var normalizedFoundPostcode = NormalizePostcode(foundPostalCode);

                if (string.IsNullOrWhiteSpace(foundPostalCode))
                {
                    return new VerifyAddressPostcodeResponse
                    {
                        IsValid = false,
                        Status = "Mismatch",
                        Message = "Google could not confirm postal code for this address.",
                        City = city,
                        State = state,
                        Country = country
                    };
                }

                if (normalizedInputPostcode != normalizedFoundPostcode)
                {
                    return new VerifyAddressPostcodeResponse
                    {
                        IsValid = false,
                        Status = "Mismatch",
                        Message = $"Address and postcode do not match. Google found postcode: {foundPostalCode}.",
                        PostalCode = foundPostalCode,
                        City = city,
                        State = state,
                        Country = country
                    };
                }

                return new VerifyAddressPostcodeResponse
                {
                    IsValid = true,
                    Status = "Valid",
                    Message = "Address and postcode verified.",
                    PostalCode = foundPostalCode,
                    City = city,
                    State = state,
                    Country = country,
                    PossibleNextAction = "ACCEPT"
                };
            }
            catch (Exception ex)
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "ProviderError",
                    Message = $"Address verification failed: {ex.Message}"
                };
            }
        }

        private static string NormalizePostcode(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }
    }
}