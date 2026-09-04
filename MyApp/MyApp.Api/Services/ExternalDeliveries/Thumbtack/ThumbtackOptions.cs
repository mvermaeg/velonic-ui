namespace MyApp.Api.Services.ExternalDeliveries.Thumbtack
{
    public sealed class ThumbtackOptions
    {
        public const string SectionName =
            "ExternalIntegrations:Thumbtack";

        public bool Enabled { get; set; } = false;

        public bool UseStaging { get; set; } = true;

        public string UtmSource { get; set; } =
            "cma-thehomeygroup";

        public string UtmMedium { get; set; } =
            "partnerships";

        public int WeeklyCapPerVertical { get; set; } = 25;

        public ThumbtackEnvironmentOptions Staging { get; set; } = new();

        public ThumbtackEnvironmentOptions Production { get; set; } = new();

        public ThumbtackVerticalOptions Roofing { get; set; } = new();

        public ThumbtackVerticalOptions Windows { get; set; } = new();

        public ThumbtackVerticalOptions Bathroom { get; set; } = new();

        public ThumbtackVerticalOptions Gutters { get; set; } = new();
    }

    public sealed class ThumbtackEnvironmentOptions
    {
        public string ApiBaseUrl { get; set; } =
            "https://api.thumbtack.com";

        public string WebsiteBaseUrl { get; set; } =
            string.Empty;

        public string TokenUrl { get; set; } =
            "https://auth.thumbtack.com/oauth2/token";

        public string Audience { get; set; } =
            "urn:partner-api";

        public string ClientId { get; set; } =
            string.Empty;

        public string ClientSecret { get; set; } =
            string.Empty;

        /*
         * Thumbtack requires a provisioned OAuth scope.
         * For this marketplace integration it should be
         * a demand::* scope.
         *
         * Leave blank until the exact assigned scope
         * is confirmed from API Reference / Account Manager.
         */
        public string Scope { get; set; } =
            string.Empty;
    }

    public sealed class ThumbtackVerticalOptions
    {
        public bool Enabled { get; set; }

        public string CategoryPk { get; set; } =
            string.Empty;

        public decimal ExpectedPayout { get; set; }
    }
}