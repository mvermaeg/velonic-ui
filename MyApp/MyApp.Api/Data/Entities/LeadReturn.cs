using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadReturn
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public long ClientId { get; set; }

    public long? LeadDeliveryId { get; set; }

    public string ReturnReason { get; set; } = null!;

    public string ReturnStatus { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ReviewedOn { get; set; }

    public string? ReviewedByUserId { get; set; }
}
