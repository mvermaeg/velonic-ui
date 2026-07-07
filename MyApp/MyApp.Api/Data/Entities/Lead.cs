using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class Lead
{
    public long Id { get; set; }

    public long? ExternalLeadId { get; set; }

    public Guid? LeadUuid { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ReceivedAt { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? IpAddress { get; set; }

    public string? CampaignName { get; set; }

    public string? AffiliateName { get; set; }

    public string? PageName { get; set; }

    public int? Step { get; set; }

    public bool IsTest { get; set; }

    public bool IsCompleted { get; set; }

    public string? Address { get; set; }

    public string? Postcode { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public decimal? Sales { get; set; }

    public decimal? Profit { get; set; }

    public string? Source { get; set; }

    public string? SourceReference { get; set; }

    public string LeadStatus { get; set; } = null!;

    public string? AssignedToUserId { get; set; }

    public DateTime? AssignedOn { get; set; }

    public string? Notes { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? UpdatedByUserId { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public long? AffiliateId { get; set; }

    public string? AffiliateSubId { get; set; }

    public long? AffiliateClickId { get; set; }

    public Guid? AffiliateClickUuid { get; set; }

    public string? EmailDomain { get; set; }

    public bool IsInvalidEmail { get; set; }

    public bool IsDisposableEmail { get; set; }

    public bool IsDuplicateLead { get; set; }

    public bool IsSuspicious { get; set; }

    public int FraudScore { get; set; }

    public string? FraudLevel { get; set; }

    public string? FraudReasons { get; set; }

    public int LeadQualityScore { get; set; }

    public string? UserAgent { get; set; }

    public string? Country { get; set; }

    public DateTime? RiskCheckedOn { get; set; }

    public bool IsPostcodeCountryMismatch { get; set; }

    public int DuplicateLeadCount { get; set; }

    public long? LastDuplicateLeadId { get; set; }

    public bool IsPostalAddressMismatch { get; set; }

    public string? PostalAddressCheckReason { get; set; }

    public bool IsVoipNumber { get; set; }

    public string? VisitorCountry { get; set; }

    public string? FingerprintHash { get; set; }

    public string? AddressValidationStatus { get; set; }

    public int? LeadTypeId { get; set; }

    public string? EmailVerificationStatus { get; set; }

    public string? EmailVerificationReason { get; set; }

    public bool? IsEmailDeliverable { get; set; }

    public virtual LeadType? LeadType { get; set; }
}
