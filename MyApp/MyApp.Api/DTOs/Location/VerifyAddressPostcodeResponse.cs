namespace MyApp.Api.DTOs.Location
{
    public class VerifyAddressPostcodeResponse
    {
        public bool IsValid { get; set; }
        public bool NeedsReview { get; set; }
        public string Status { get; set; } = "";
        public string Message { get; set; } = "";
        public string? FormattedAddress { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PossibleNextAction { get; set; }
    }
}