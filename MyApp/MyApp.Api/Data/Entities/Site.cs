using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class Site
{
    public long Id { get; set; }

    public string SiteName { get; set; } = null!;

    public string? DomainName { get; set; }

    public string Slug { get; set; } = null!;

    public string ThemeKey { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public int? WebsiteTemplateId { get; set; }

    public int? WebsiteThemeId { get; set; }

    public virtual ICollection<SiteDomain> SiteDomains { get; set; } = new List<SiteDomain>();

    public virtual ICollection<SiteForm> SiteForms { get; set; } = new List<SiteForm>();

    public virtual ICollection<SiteMediaFile> SiteMediaFiles { get; set; } = new List<SiteMediaFile>();

    public virtual ICollection<SitePage> SitePages { get; set; } = new List<SitePage>();

    public virtual ICollection<SitePixelEvent> SitePixelEvents { get; set; } = new List<SitePixelEvent>();

    public virtual ICollection<SitePixel> SitePixels { get; set; } = new List<SitePixel>();

    public virtual ICollection<SiteSetting> SiteSettings { get; set; } = new List<SiteSetting>();

    public virtual WebsiteTemplate? WebsiteTemplate { get; set; }

    public virtual WebsiteTheme? WebsiteTheme { get; set; }
}
