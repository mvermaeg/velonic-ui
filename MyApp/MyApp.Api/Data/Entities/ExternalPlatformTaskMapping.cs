using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class ExternalPlatformTaskMapping
{
    public long Id { get; set; }

    public string PlatformCode { get; set; } = null!;

    public int LeadTypeId { get; set; }

    public string ExternalTaskId { get; set; } = null!;

    public string? ExternalTaskName { get; set; }

    public string? VerticalCode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public string? InternalOptionCode { get; set; }

    public virtual LeadType LeadType { get; set; } = null!;
}
