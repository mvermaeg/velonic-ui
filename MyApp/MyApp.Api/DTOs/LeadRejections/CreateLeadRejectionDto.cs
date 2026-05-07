namespace MyApp.Api.DTOs.LeadRejections
{
    public class CreateLeadRejectionDto
    {
        public long LeadId { get; set; }

        public string RejectionReason { get; set; } = string.Empty;

        public string? RejectionSource { get; set; }

        public string? Notes { get; set; }

        public string? CreatedByUserId { get; set; }
    }
}