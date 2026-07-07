using System.Text.Json;

namespace MyApp.Api.Services.EmailValidation
{
    public class BouncerEmailValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BouncerEmailValidationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<BouncerResponse?> VerifyAsync(string email)
        {
            var apiKey = _configuration["Bouncer:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new BouncerResponse
                {
                    Email = email,
                    Status = "ProviderError",
                    Reason = "Bouncer API key missing"
                };
            }

            var url =
                $"https://api.usebouncer.com/v1.1/email/verify?email={Uri.EscapeDataString(email)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-api-key", apiKey);

            using var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new BouncerResponse
                {
                    Email = email,
                    Status = "ProviderError",
                    Reason = json
                };
            }

            return JsonSerializer.Deserialize<BouncerResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
    }
}