using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class AffiliateClick
{
    public long Id { get; set; }

    public Guid ClickUuid { get; set; }

    public long? AffiliateId { get; set; }

    public string? AffiliateName { get; set; }

    public string? ApiKey { get; set; }

    public string? SubId { get; set; }

    public string? PageName { get; set; }

    public string? CampaignName { get; set; }

    public string? SourceName { get; set; }

    public string? Url { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedOn { get; set; }
}
