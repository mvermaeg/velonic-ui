namespace MyApp.Api.Services.ExternalDeliveries.BlueInk
{
    public class BlueInkOptions
    {
        public const string SectionName = "ExternalIntegrations:BlueInk";

        public bool Enabled { get; set; }

        public bool UsePingPost { get; set; } = true;

        public bool UseTestEndpoints { get; set; } = true;

        public Dictionary<string, BlueInkVerticalOptions> Verticals { get; set; }
            = new(StringComparer.OrdinalIgnoreCase);
    }

    public class BlueInkVerticalOptions
    {
        public bool Enabled { get; set; } = true;

        public string Token { get; set; } = string.Empty;

        public string TestPingUrl { get; set; } =
            "https://exchange.standardinformation.io/ping_test?legacy=J";

        public string TestPostUrl { get; set; } =
            "https://exchange.standardinformation.io/post_test?legacy=J";

        public string ProductionPingUrl { get; set; } =
            "https://exchange.standardinformation.io/ping?legacy=J";

        public string ProductionPostUrl { get; set; } =
            "https://exchange.standardinformation.io/post?legacy=J";
    }
}