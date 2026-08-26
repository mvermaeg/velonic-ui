namespace MyApp.Api.Services.Ringba
{
    public class RingbaOptions
    {
        public const string SectionName = "ExternalIntegrations:Ringba";

        public bool Enabled { get; set; } = true;

        public string BaseUrl { get; set; } =
            "https://rtb.ringba.com/v1/production";

        public Dictionary<string, string> RtbIds { get; set; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}