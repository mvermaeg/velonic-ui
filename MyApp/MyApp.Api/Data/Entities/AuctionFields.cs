namespace MyApp.Api.Data.Entities;

public partial class LeadRoutingRun
{
    public string RoutingMode { get; set; } = "Legacy";
    public DateTime? AuctionStartedOn { get; set; }
    public DateTime? AuctionClosesOn { get; set; }
    public DateTime? WinnerSelectedOn { get; set; }
    public DateTime? WinnerLockedOn { get; set; }
    public DateTime? PostCompletedOn { get; set; }
    public long? WinnerAttemptId { get; set; }
    public string? ThumbtackEligibility { get; set; }
    public string? ThumbtackSearchId { get; set; }
    public string? ThumbtackResponseJson { get; set; }
    public DateTime? ThumbtackSearchStartedOn { get; set; }
    public DateTime? ReconciledOn { get; set; }
    public string? ReconciledBy { get; set; }
    public string? ReconciliationReference { get; set; }
    public string? ReconciliationOutcome { get; set; }
}
public partial class LeadRoutingAttempt
{
    public bool IsWinner { get; set; }
    public DateTime? PingDispatchedOn { get; set; }
    public DateTime? BidReceivedOn { get; set; }
    public DateTime? BidExpiresOn { get; set; }
    public decimal? OfferedBidAmount { get; set; }
    public long? ResponseDurationMilliseconds { get; set; }
    public string? PingReferenceId { get; set; }
    public string? PostContext { get; set; }
    public string? PingRequestAudit { get; set; }
    public string? PingResponseAudit { get; set; }
    public string? PostRequestAudit { get; set; }
    public string? PostResponseAudit { get; set; }
    public DateTime? PostStartedOn { get; set; }
    public int PostAttempts { get; set; }
}
public partial class LeadRoutingRule
{
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
