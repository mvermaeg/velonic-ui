using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadRejection
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public string RejectionReason { get; set; } = null!;

    public string? RejectionSource { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? CreatedByUserId { get; set; }
}
