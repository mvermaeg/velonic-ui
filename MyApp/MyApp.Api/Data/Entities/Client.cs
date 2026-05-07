using System;
using System.Collections.Generic;

namespace MyApp.Api.Data.Entities;

public partial class Client
{
    public long Id { get; set; }

    public string ClientName { get; set; } = null!;

    public string? Tier { get; set; }

    public string AccountStatus { get; set; } = null!;

    public string? Timezone { get; set; }

    public bool AcceptsWebLeads { get; set; }

    public bool AcceptsInboundCalls { get; set; }

    public bool IsBuyer { get; set; }

    public bool IsVendor { get; set; }

    public string? AccountManagerUserId { get; set; }

    public string? SubAccountManagerUserId { get; set; }

    public string? ReturnAgreement { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }
}
