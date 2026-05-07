namespace MyApp.Api.DTOs.Clients
{
    public class CreateClientDto
    {
        public string ClientName { get; set; } = string.Empty;

        public string? Tier { get; set; }

        public string AccountStatus { get; set; } = "Active";

        public string? Timezone { get; set; }

        public bool AcceptsWebLeads { get; set; } = true;

        public bool AcceptsInboundCalls { get; set; } = false;

        public bool IsBuyer { get; set; } = true;

        public bool IsVendor { get; set; } = false;

        public string? AccountManagerUserId { get; set; }

        public string? SubAccountManagerUserId { get; set; }

        public string? ReturnAgreement { get; set; }
    }
}