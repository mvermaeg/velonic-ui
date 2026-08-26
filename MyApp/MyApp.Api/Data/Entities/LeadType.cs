using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<ExternalPlatformTaskMapping> ExternalPlatformTaskMappings { get; set; } = new List<ExternalPlatformTaskMapping>();

    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
