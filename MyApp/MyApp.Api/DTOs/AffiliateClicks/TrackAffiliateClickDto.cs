namespace MyApp.Api.DTOs.AffiliateClicks
{
    public class TrackAffiliateClickDto
    {
        public string? ApiKey { get; set; }
        public string? SubId { get; set; }
        public string? PageName { get; set; }
        public string? CampaignName { get; set; }
        public string? SourceName { get; set; }
        public string? Url { get; set; }
    }
}