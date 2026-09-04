namespace MyApp.Api.Services.Thumbtack
{
    public sealed class ThumbtackOptions
    {
        public const string SectionName =
            "ExternalIntegrations:Thumbtack";

        public bool Enabled { get; set; }

        public bool UseStaging { get; set; } = true;

        public string StagingBaseUrl { get; set; } =
            "https://staging-partner.thumbtack.com";

        public string ProductionBaseUrl { get; set; } =
            "https://thumbtack.com";

        public string ApiBaseUrl { get; set; } =
            "https://api.thumbtack.com";

        public string UtmSource { get; set; } =
            "cma-thehomeygroup";

        public string UtmMedium { get; set; } =
            "partnership";

        public int WeeklyCapPerCategory { get; set; } = 25;

        public ThumbtackCategoryOptions Roofing { get; set; } = new();
        public ThumbtackCategoryOptions Windows { get; set; } = new();
        public ThumbtackCategoryOptions Bathroom { get; set; } = new();
        public ThumbtackCategoryOptions Gutters { get; set; } = new();
    }

    public sealed class ThumbtackCategoryOptions
    {
        public bool Enabled { get; set; }

        public string CategoryPk { get; set; } =
            string.Empty;

        public decimal ExpectedPayout { get; set; }
    }
}