using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadStatusHistory
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = null!;

    public string? ChangedByUserId { get; set; }

    public DateTime ChangedOn { get; set; }

    public string? Remarks { get; set; }
}
