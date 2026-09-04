namespace MyApp.Api.Services.ExternalDeliveries.Modernize
{
    public sealed class ModernizeOptions
    {
        public const string SectionName =
            "ExternalIntegrations:Modernize";

        public bool Enabled { get; set; }

        public bool UseStaging { get; set; } = true;

        public string StagingBaseUrl { get; set; } =
            "https://hsapiservice.quinstage.com";

        public string ProductionBaseUrl { get; set; } =
            "https://form-service-hs.qnst.com";

        public string StagingTagId { get; set; } =
            "204670250";

        public string ProductionTagId { get; set; }
            = string.Empty;

        public string PartnerSourceId { get; set; }
            = "Homeyy";
    }
}