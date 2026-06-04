using System.Text.Json.Serialization;

namespace MyApp.Api.Services.Location
{
    public class IpLocationService
    {
        private readonly HttpClient _httpClient;

        public IpLocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IpLocationResult?> GetAsync(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return null;

            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                return null;

            try
            {
                return await _httpClient.GetFromJsonAsync<IpLocationResult>(
                    $"https://ipapi.co/{ipAddress}/json/");
            }
            catch
            {
                return null;
            }
        }
    }

    public class IpLocationResult
    {
        [JsonPropertyName("country_name")]
        public string? CountryName { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }
    }
}