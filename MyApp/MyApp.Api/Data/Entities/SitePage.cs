using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SitePage
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string PageName { get; set; } = null!;

    public string PageSlug { get; set; } = null!;

    public string? PageTitle { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? HeroTitle { get; set; }

    public string? HeroSubtitle { get; set; }

    public string? HtmlContent { get; set; }

    public string? JsonContent { get; set; }

    public bool IsHomePage { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<SiteForm> SiteForms { get; set; } = new List<SiteForm>();

    public virtual ICollection<SitePageSection> SitePageSections { get; set; } = new List<SitePageSection>();
}
