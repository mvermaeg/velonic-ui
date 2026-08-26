namespace MyApp.Api.Services.ExternalDeliveries.Networx
{
    public class NetworxOptions
    {
        public const string SectionName =
            "ExternalIntegrations:Networx";

        public bool Enabled { get; set; }

        public bool UsePingPost { get; set; } = true;

        public bool UseTestMode { get; set; } = true;

        public string BaseUrl { get; set; } =
            "https://api.networx.com";

        public string UserId { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SourceId { get; set; } = "Homeyy";
    }
}