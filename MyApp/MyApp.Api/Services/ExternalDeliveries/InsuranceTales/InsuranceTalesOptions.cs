namespace MyApp.Api.Services.ExternalDeliveries.InsuranceTales
{
    public sealed class InsuranceTalesOptions
    {
        public const string SectionName =
            "ExternalIntegrations:InsuranceTales";

        public bool Enabled { get; set; }

        public bool UseTestMode { get; set; } = true;

        public string PingUrl { get; set; } =
            "https://insurancetales.leadspediatrack.com/ping.do";

        public string PostUrl { get; set; } =
            "https://insurancetales.leadspediatrack.com/post.do";

        public InsuranceTalesVerticalOptions Windows { get; set; }
            = new();
    }

    public sealed class InsuranceTalesVerticalOptions
    {
        public bool Enabled { get; set; }

        public string CampaignId { get; set; } = string.Empty;

        public string CampaignKey { get; set; } = string.Empty;
    }
}