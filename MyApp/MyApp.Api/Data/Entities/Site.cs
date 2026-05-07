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

    public virtual WebsiteTemplate? WebsiteTemplate { get; set; }

    public virtual WebsiteTheme? WebsiteTheme { get; set; }
}
