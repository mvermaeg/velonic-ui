using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadRoutingAttempt
{
    public long Id { get; set; }

    public long LeadRoutingRunId { get; set; }

    public long LeadId { get; set; }

    public long? LeadRoutingRuleId { get; set; }

    public string DestinationType { get; set; } = null!;

    public long? ClientId { get; set; }

    public string? PlatformCode { get; set; }

    public decimal BidAmount { get; set; }

    public int AttemptNumber { get; set; }

    public string Status { get; set; } = null!;

    public bool IsRetryable { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public virtual Lead Lead { get; set; } = null!;

    public virtual LeadRoutingRule? LeadRoutingRule { get; set; }

    public virtual LeadRoutingRun LeadRoutingRun { get; set; } = null!;
}
