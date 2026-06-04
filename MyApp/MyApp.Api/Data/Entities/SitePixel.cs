using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SitePixel
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string PixelType { get; set; } = null!;

    public string? PixelName { get; set; }

    public string? PixelCode { get; set; }

    public string Placement { get; set; } = null!;

    public string? CampaignName { get; set; }

    public string? SourceName { get; set; }

    public string? FireOnPageSlug { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<SitePixelEvent> SitePixelEvents { get; set; } = new List<SitePixelEvent>();
}
