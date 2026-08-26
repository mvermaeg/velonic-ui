namespace MyApp.Api.DTOs.Leads
{
    public class WebsiteEmailValidationDto
    {
        public string? Email { get; set; }
    }

    public class WebsiteAddressValidationDto
    {
        public string? Address { get; set; }

        public string? Postcode { get; set; }
    }
}