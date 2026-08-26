namespace MyApp.Api.DTOs.Ringba
{
    public class RingbaBidRequestDto
    {
        public string Vertical { get; set; } = string.Empty;

        public string CallerPhone { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public string? SubId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }
    }
}