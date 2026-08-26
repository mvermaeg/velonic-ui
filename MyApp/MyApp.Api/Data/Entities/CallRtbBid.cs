using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class CallRtbBid
{
    public long Id { get; set; }

    public long CallRtbRequestId { get; set; }

    public string? BidId { get; set; }

    public decimal? BidAmount { get; set; }

    public int? ExpireInSeconds { get; set; }

    public DateTime? BidExpireDateTime { get; set; }

    public long? BidExpireEpoch { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PhoneNumberNoPlus { get; set; }

    public string? SipAddress { get; set; }

    public string? BidTermsJson { get; set; }

    public string? WarningsJson { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual CallRtbRequest CallRtbRequest { get; set; } = null!;
}
