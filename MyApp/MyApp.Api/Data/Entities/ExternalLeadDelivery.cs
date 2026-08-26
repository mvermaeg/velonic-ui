using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class ExternalLeadDelivery
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public string PlatformCode { get; set; } = null!;

    public string? VerticalCode { get; set; }

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public int MaxAttempts { get; set; }

    public DateTime? NextAttemptOn { get; set; }

    public DateTime? LastAttemptOn { get; set; }

    public DateTime? DeliveredOn { get; set; }

    public int? HttpStatusCode { get; set; }

    public string? ExternalReferenceId { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual Lead Lead { get; set; } = null!;
}
