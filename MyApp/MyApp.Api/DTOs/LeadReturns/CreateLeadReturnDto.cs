namespace MyApp.Api.DTOs.LeadReturns
{
    public class CreateLeadReturnDto
    {
        public long LeadId { get; set; }

        public long ClientId { get; set; }

        public long? LeadDeliveryId { get; set; }

        public string ReturnReason { get; set; } = string.Empty;

        public string? Notes { get; set; }
    }
}