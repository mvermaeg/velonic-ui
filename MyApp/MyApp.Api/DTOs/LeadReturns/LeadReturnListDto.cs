namespace MyApp.Api.DTOs.LeadReturns
{
    public class LeadReturnListDto
    {
        public long Id { get; set; }

        public long LeadId { get; set; }

        public Guid? LeadUuid { get; set; }

        public long ClientId { get; set; }

        public string? ClientName { get; set; }

        public long? LeadDeliveryId { get; set; }

        public string ReturnReason { get; set; } = string.Empty;

        public string ReturnStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ReviewedOn { get; set; }

        public string? Email { get; set; }

        public string? CampaignName { get; set; }

        public string? SourceName { get; set; }
    }
}