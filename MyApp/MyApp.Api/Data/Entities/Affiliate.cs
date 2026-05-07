using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class Affiliate
{
    public long Id { get; set; }

    public string AffiliateName { get; set; } = null!;

    public string? ApiKey { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
