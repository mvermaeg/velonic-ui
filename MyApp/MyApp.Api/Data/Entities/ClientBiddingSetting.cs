using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class ClientBiddingSetting
{
    public long Id { get; set; }

    public long ClientId { get; set; }

    public string? LeadType { get; set; }

    public string? State { get; set; }

    public string? Postcode { get; set; }

    public decimal BidAmount { get; set; }

    public int? DailyCap { get; set; }

    public int? MonthlyCap { get; set; }

    public bool IsExclusive { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}
