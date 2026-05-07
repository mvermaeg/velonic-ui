namespace MyApp.Api.DTOs.Leads
{
    public class LeadListDto
    {
        public long Id { get; set; }
        public Guid? LeadUuid { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? CampaignName { get; set; }
        public string? AffiliateName { get; set; }
        public string? PageName { get; set; }

        public string? Postcode { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }

        public int? Step { get; set; }
        public bool IsCompleted { get; set; }

        public string? LeadStatus { get; set; }
    }
}