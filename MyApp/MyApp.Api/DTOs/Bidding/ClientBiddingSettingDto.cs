namespace MyApp.Api.DTOs.Bidding
{
    public class ClientBiddingSettingDto
    {
        public long ClientId { get; set; }

        public string? LeadType { get; set; }

        public string? State { get; set; }

        public string? Postcode { get; set; }

        public decimal BidAmount { get; set; }

        public int? DailyCap { get; set; }

        public int? MonthlyCap { get; set; }

        public bool IsExclusive { get; set; }

        public bool IsActive { get; set; }
    }
}