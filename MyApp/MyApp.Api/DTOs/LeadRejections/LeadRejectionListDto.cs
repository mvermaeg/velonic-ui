namespace MyApp.Api.DTOs.LeadRejections
{
    public class LeadRejectionListDto
    {
        public long Id { get; set; }

        public long LeadId { get; set; }

        public Guid? LeadUuid { get; set; }

        public string? Email { get; set; }

        public string? FullName { get; set; }

        public string? CampaignName { get; set; }

        public string? SourceName { get; set; }

        public string RejectionReason { get; set; } = string.Empty;

        public string? RejectionSource { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? CreatedByUserId { get; set; }
    }
}