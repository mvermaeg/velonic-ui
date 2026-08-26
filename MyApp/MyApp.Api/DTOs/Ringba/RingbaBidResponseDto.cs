namespace MyApp.Api.DTOs.Ringba
{
    public class RingbaBidResponseDto
    {
        public bool Success { get; set; }

        public string? BidId { get; set; }

        public decimal? BidAmount { get; set; }

        public int? ExpireInSeconds { get; set; }

        public string? BidExpireDateTime { get; set; }

        public long? BidExpireEpoch { get; set; }

        public string? PhoneNumber { get; set; }

        public string? PhoneNumberNoPlus { get; set; }

        public string? SipAddress { get; set; }

        public string? BidTermsJson { get; set; }

        public string? WarningsJson { get; set; }

        public int? HttpStatusCode { get; set; }

        public string? RawResponse { get; set; }

        public string? Error { get; set; }
    }
}