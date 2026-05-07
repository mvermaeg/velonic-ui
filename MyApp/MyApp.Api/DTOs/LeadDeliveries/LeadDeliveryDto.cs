namespace MyApp.Api.DTOs.LeadDeliveries
{
    public class LeadDeliveryDto
    {
        public long Id { get; set; }

        public long LeadId { get; set; }

        public Guid? LeadUuid { get; set; }

        public long ClientId { get; set; }

        public string? ClientName { get; set; }

        public long? LeadBiddingResultId { get; set; }

        public string DeliveryType { get; set; } = string.Empty;

        public string DeliveryStatus { get; set; } = string.Empty;

        public DateTime? DeliveredOn { get; set; }

        public string? ResponseMessage { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? Email { get; set; }

        public string? CampaignName { get; set; }

        public string? SourceName { get; set; }
    }
}