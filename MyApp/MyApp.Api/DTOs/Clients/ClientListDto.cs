namespace MyApp.Api.DTOs.Clients
{
    public class ClientListDto
    {
        public long Id { get; set; }

        public string ClientName { get; set; } = string.Empty;

        public string? Tier { get; set; }

        public string AccountStatus { get; set; } = string.Empty;

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
    }
}