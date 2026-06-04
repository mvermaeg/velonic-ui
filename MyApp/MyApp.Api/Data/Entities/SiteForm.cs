using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class SiteForm
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public long? SitePageId { get; set; }

    public string FormKey { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? SubmitButtonText { get; set; }

    public string? SuccessMessage { get; set; }

    public string? CampaignName { get; set; }

    public string? SourceName { get; set; }

    public bool IsMultiStep { get; set; }

    public string? SettingsJson { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual Site Site { get; set; } = null!;

    public virtual SitePage? SitePage { get; set; }
}
