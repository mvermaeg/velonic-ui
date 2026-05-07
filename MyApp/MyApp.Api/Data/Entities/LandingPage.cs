using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LandingPage
{
    public long Id { get; set; }

    public long SiteId { get; set; }

    public string PageName { get; set; } = null!;

    public string PageSlug { get; set; } = null!;

    public string? CampaignName { get; set; }

    public string? AffiliateName { get; set; }

    public string? PageTitle { get; set; }

    public string? HeroTitle { get; set; }

    public string? HeroSubtitle { get; set; }

    public string? CtaText { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
