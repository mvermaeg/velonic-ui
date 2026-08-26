namespace MyApp.Api.Services.TwilioVoice
{
    public class TwilioVoiceOptions
    {
        public const string SectionName = "TwilioVoice";

        // Master switch for this complete module.
        public bool Enabled { get; set; } = false;

        // When false, no Ringba production bid is requested.
        public bool EnableRingbaRouting { get; set; } = false;

        // Set true only after the public HTTPS URL is final.
        public bool ValidateTwilioSignature { get; set; } = false;

        // Example: https://auth.homeyy.com
        public string WebhookBaseUrl { get; set; } = string.Empty;

        // Optional destination when Ringba is disabled or returns no bid.
        public string? FallbackNumber { get; set; }

        public string SubId { get; set; } = "HOMEYY";

        public string Voice { get; set; } = "alice";
    }
}