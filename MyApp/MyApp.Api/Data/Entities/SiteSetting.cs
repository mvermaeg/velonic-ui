using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SiteSetting
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string? LogoUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? EmailAddress { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? BusinessHours { get; set; }

    public string? FacebookUrl { get; set; }

    public string? LinkedinUrl { get; set; }

    public string? InstagramUrl { get; set; }

    public string? TwitterUrl { get; set; }

    public string? YoutubeUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;
}
