using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadSource
{
    public long Id { get; set; }

    public string SourceName { get; set; } = null!;

    public string SourceType { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
