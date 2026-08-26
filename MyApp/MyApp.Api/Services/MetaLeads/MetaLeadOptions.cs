namespace MyApp.Api.Services.MetaLeads
{
    public class MetaLeadOptions
    {
        public const string SectionName = "MetaLeadAds";

        public bool Enabled { get; set; } = false;

        public string VerifyToken { get; set; } = string.Empty;

        public string AppSecret { get; set; } = string.Empty;

        // Put the API version shown in your Meta App dashboard.
        // Example format: vXX.X
        public string GraphApiVersion { get; set; } = string.Empty;

        public Dictionary<string, MetaPageOptions> Pages { get; set; }
            = new();
    }

    public class MetaPageOptions
    {
        public string PageName { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public int LeadTypeId { get; set; }

        // Optional:
        // if one Meta page has forms for different verticals.
        public Dictionary<string, int> FormLeadTypeIds { get; set; }
            = new();
    }
}