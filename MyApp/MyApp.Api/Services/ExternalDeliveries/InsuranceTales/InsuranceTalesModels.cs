namespace MyApp.Api.Services.ExternalDeliveries.InsuranceTales
{
    public sealed class InsuranceTalesPingResponse
    {
        public bool Success { get; set; }

        public string? PingId { get; set; }

        public decimal? Price { get; set; }

        public string? Message { get; set; }

        public string? Error { get; set; }

        public string? RawResponse { get; set; }
    }

    public sealed class InsuranceTalesPostResponse
    {
        public bool Success { get; set; }

        public string? LeadId { get; set; }

        public decimal? Price { get; set; }

        public string? RedirectUrl { get; set; }

        public string? Message { get; set; }

        public string? Error { get; set; }

        public string? RawResponse { get; set; }
    }
}   