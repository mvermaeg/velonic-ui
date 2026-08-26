namespace MyApp.Api.DTOs.Leads
{
    public class WebsiteLeadCreateDto
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? CampaignName { get; set; }

        public string? AffiliateName { get; set; }

        public string? PageName { get; set; }

        public string? Address { get; set; }

        public string? Postcode { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public int? Step { get; set; }

        public bool IsTest { get; set; }

        public bool IsCompleted { get; set; } = true;
        public string? AffiliateSubId { get; set; }
        public string? FingerprintHash { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public int? LeadTypeId { get; set; }



        //=======
        public string? TcpaComplianceText { get; set; }

        public string? TrustedFormCertificateUrl { get; set; }

        public string? JornayaLeadId { get; set; }

        public string? LandingPageUrl { get; set; }

        public DateTime? ConsentCapturedOn { get; set; }

        public bool? IsTcpaCompliant { get; set; }

        public bool? OwnsProperty { get; set; }

        public string? AdditionalDataJson { get; set; }
        public string? ServiceCode { get; set; }
    }
}