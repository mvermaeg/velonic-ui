using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadAttempt
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public int AttemptNo { get; set; }

    public string? AttemptType { get; set; }

    public string? AttemptedByUserId { get; set; }

    public DateTime AttemptedOn { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }
}
