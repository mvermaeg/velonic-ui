using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class Organization
{
    public long Id { get; set; }

    public string OrganizationName { get; set; } = null!;

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
