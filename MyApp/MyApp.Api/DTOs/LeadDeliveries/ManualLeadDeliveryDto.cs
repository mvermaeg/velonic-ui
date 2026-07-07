namespace MyApp.Api.DTOs.LeadDelivery
{
    public class ManualLeadDeliveryDto
    {
        public long LeadId { get; set; }
        public long ClientId { get; set; }
        public decimal BidAmount { get; set; } = 0;
        public string? Note { get; set; }
    }
}