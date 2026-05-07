using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class WebsiteTheme
{
    public int Id { get; set; }

    public string ThemeName { get; set; } = null!;

    public string ThemeKey { get; set; } = null!;

    public string? PrimaryColor { get; set; }

    public string? SecondaryColor { get; set; }

    public string? FontFamily { get; set; }

    public string? LogoUrl { get; set; }

    public string? CustomCss { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
}
