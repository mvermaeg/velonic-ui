using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadForm
{
    public long Id { get; set; }

    public long? LeadSourceId { get; set; }

    public string? ExternalFormId { get; set; }

    public string FormName { get; set; } = null!;

    public string? PageName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
