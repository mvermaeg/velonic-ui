namespace MyApp.Api.DTOs.ClientBidding
{
    public class LeadBiddingResultDto
    {
        public long Id { get; set; }

        public long LeadId { get; set; }

        public Guid? LeadUuid { get; set; }

        public long ClientId { get; set; }

        public string? ClientName { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LeadCreatedAt { get; set; }

        public string? Email { get; set; }

        public string? CampaignName { get; set; }

        public string? SourceName { get; set; }

        public decimal BidAmount { get; set; }

        public bool IsWon { get; set; }

        public bool IsSold { get; set; }

        public DateTime? SoldOn { get; set; }

        public string? MatchReason { get; set; }
    }
}