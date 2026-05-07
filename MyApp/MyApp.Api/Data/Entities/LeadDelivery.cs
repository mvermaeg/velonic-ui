using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadDelivery
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public long ClientId { get; set; }

    public long? LeadBiddingResultId { get; set; }

    public string DeliveryType { get; set; } = null!;

    public string DeliveryStatus { get; set; } = null!;

    public DateTime? DeliveredOn { get; set; }

    public string? ResponseMessage { get; set; }

    public DateTime CreatedOn { get; set; }
}
