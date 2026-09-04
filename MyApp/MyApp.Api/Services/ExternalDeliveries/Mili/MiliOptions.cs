namespace MyApp.Api.Services.ExternalDeliveries.Mili
{
    public sealed class MiliOptions
    {
        public const string SectionName = "ExternalIntegrations:Mili";

        public bool Enabled { get; set; }

        public bool UseTestMode { get; set; } = true;

        public string PingUrl { get; set; } =
            "https://miligroup.leadspediatrack.com/ping.do";

        public string PostUrl { get; set; } =
            "https://miligroup.leadspediatrack.com/post.do";

        public string PubId { get; set; } = string.Empty;

        // Shared because Homeyy may distribute the same lead
        // to multiple external platforms.
        public string LeadType { get; set; } = "Shared";

        public MiliVerticalOptions Bathroom { get; set; } = new();

        public MiliVerticalOptions HVAC { get; set; } = new();

        public MiliVerticalOptions Roofing { get; set; } = new();

        public MiliVerticalOptions Windows { get; set; } = new();
    }

    public sealed class MiliVerticalOptions
    {
        public bool Enabled { get; set; }

        public string CampaignId { get; set; } = string.Empty;

        public string CampaignKey { get; set; } = string.Empty;
    }
}