using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadCampaign
{
    public long Id { get; set; }

    public long? LeadSourceId { get; set; }

    public string? ExternalCampaignId { get; set; }

    public string CampaignName { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }
}
