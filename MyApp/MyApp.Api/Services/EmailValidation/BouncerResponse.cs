using System.Text.Json.Serialization;

namespace MyApp.Api.Services.EmailValidation
{
    public class BouncerResponse
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("score")]
        public decimal? Score { get; set; }

        public bool Deliverable =>
            string.Equals(Status, "deliverable", StringComparison.OrdinalIgnoreCase);
    }
}