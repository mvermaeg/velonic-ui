using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadRoutingRule
{
    public long Id { get; set; }

    public string DestinationType { get; set; } = null!;

    public long? ClientId { get; set; }

    public string? PlatformCode { get; set; }

    public string VerticalCode { get; set; } = null!;

    public string? State { get; set; }

    public string? Postcode { get; set; }

    public decimal BidAmount { get; set; }

    public int Priority { get; set; }

    public int? DailyCap { get; set; }

    public int? MonthlyCap { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual ICollection<LeadRoutingAttempt> LeadRoutingAttempts { get; set; } = new List<LeadRoutingAttempt>();
}
