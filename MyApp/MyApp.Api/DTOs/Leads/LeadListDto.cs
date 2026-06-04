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
        public string? Country { get; set; }
        public string? IpAddress { get; set; }
        public int DuplicateLeadCount { get; set; }
        public long? LastDuplicateLeadId { get; set; }
        public string? VisitorCountry { get; set; }
        public string? FingerprintHash { get; set; }
        public bool IsVoipNumber { get; set; }
        public int? Step { get; set; }
        public bool IsCompleted { get; set; }

        public string? LeadStatus { get; set; }

        public int FraudScore { get; set; }
        public int LeadQualityScore { get; set; }
        public string? FraudLevel { get; set; }
        public string? FraudReasons { get; set; }

        public bool IsInvalidEmail { get; set; }
        public bool IsDisposableEmail { get; set; }
        public bool IsDuplicateLead { get; set; }
        public bool IsSuspicious { get; set; }
        public bool IsPostcodeCountryMismatch { get; set; }
    }
}