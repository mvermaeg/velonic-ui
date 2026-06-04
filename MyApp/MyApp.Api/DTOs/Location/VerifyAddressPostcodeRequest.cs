namespace MyApp.Api.DTOs.Location
{
    public class VerifyAddressPostcodeRequest
    {
        public string Address { get; set; } = "";
        public string Postcode { get; set; } = "";
        public string? CountryCode { get; set; } = "IN";
    }
}