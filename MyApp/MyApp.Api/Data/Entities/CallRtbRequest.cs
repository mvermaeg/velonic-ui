using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class CallRtbRequest
{
    public long Id { get; set; }

    public string VerticalCode { get; set; } = null!;

    public string ProviderCode { get; set; } = null!;

    public string CallerPhone { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public string? SubId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? RequestPayload { get; set; }

    public string? ResponsePayload { get; set; }

    public string Status { get; set; } = null!;

    public int? HttpStatusCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public virtual ICollection<CallRtbBid> CallRtbBids { get; set; } = new List<CallRtbBid>();
}
