using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadRawPayload
{
    public long Id { get; set; }

    public long? LeadId { get; set; }

    public string? SourceName { get; set; }

    public string? ExternalLeadId { get; set; }

    public string RawJson { get; set; } = null!;

    public DateTime ReceivedOn { get; set; }
}
