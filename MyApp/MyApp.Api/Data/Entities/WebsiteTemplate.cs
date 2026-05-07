using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class WebsiteTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public string TemplateKey { get; set; } = null!;

    public string? PreviewImage { get; set; }

    public string? SourceFolder { get; set; }

    public string? BuildFolder { get; set; }

    public string? Version { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();
}
