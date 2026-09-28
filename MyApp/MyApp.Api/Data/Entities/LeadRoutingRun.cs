using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadRoutingRun
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public string VerticalCode { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? WinnerType { get; set; }

    public long? WinnerClientId { get; set; }

    public string? WinnerPlatformCode { get; set; }

    public decimal? WinningBidAmount { get; set; }

    public DateTime? NextAttemptOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Lead Lead { get; set; } = null!;

    public virtual ICollection<LeadRoutingAttempt> LeadRoutingAttempts { get; set; } = new List<LeadRoutingAttempt>();
}
