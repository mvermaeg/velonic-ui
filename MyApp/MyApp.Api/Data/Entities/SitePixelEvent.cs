using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SitePixelEvent
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public long? SitePixelId { get; set; }

    public string? PageSlug { get; set; }

    public string EventName { get; set; } = null!;

    public string? Url { get; set; }

    public string? UserAgent { get; set; }

    public string? Referrer { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;

    public virtual SitePixel? SitePixel { get; set; }
}
