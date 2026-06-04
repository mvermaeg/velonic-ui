using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SiteMediaFile
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public string? MimeType { get; set; }

    public long? FileSizeBytes { get; set; }

    public string? AltText { get; set; }

    public string? Caption { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;
}
