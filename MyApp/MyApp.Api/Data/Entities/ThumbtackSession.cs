using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class ThumbtackSession
{
    public long Id { get; set; }

    public long? LeadId { get; set; }

    public string TrackingId { get; set; } = null!;

    public string UtmContent { get; set; } = null!;

    public string CategoryCode { get; set; } = null!;

    public string? CategoryPk { get; set; }

    public string? ZipCode { get; set; }

    public string Status { get; set; } = null!;

    public string? BusinessId { get; set; }

    public string? ServicePk { get; set; }

    public string? RequestFlowUrl { get; set; }

    public string? ReportedOutcome { get; set; }

    public decimal? ExpectedPayout { get; set; }

    public decimal? ActualPayout { get; set; }

    public DateTime? StartedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public DateTime? LastReportSyncOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
