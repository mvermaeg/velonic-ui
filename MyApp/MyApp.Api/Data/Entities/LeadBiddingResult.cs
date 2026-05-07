using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class LeadBiddingResult
{
    public long Id { get; set; }

    public long LeadId { get; set; }

    public long ClientId { get; set; }

    public long? ClientBiddingSettingId { get; set; }

    public decimal BidAmount { get; set; }

    public string? MatchReason { get; set; }

    public bool IsWon { get; set; }

    public bool IsSold { get; set; }

    public DateTime? SoldOn { get; set; }

    public DateTime CreatedOn { get; set; }
}
