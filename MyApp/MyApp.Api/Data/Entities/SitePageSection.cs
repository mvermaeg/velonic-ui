using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SitePageSection
{
    public long Id { get; set; }

    public long SitePageId { get; set; }

    public string SectionKey { get; set; } = null!;

    public string? SectionTitle { get; set; }

    public string? SectionSubtitle { get; set; }

    public string? HtmlContent { get; set; }

    public string? ImageUrl { get; set; }

    public string? ButtonText { get; set; }

    public string? ButtonUrl { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual SitePage SitePage { get; set; } = null!;
}
