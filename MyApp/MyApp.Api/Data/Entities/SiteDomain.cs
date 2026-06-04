using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SiteDomain
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string DomainName { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public bool SslEnabled { get; set; }

    public bool CloudflareEnabled { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;
}
