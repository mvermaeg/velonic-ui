namespace MyApp.Api.Services.ExternalDeliveries
{
    public class ExternalLeadDeliveryResult
    {
        public bool IsSuccess { get; set; }

        public bool IsRetryable { get; set; }

        public int? HttpStatusCode { get; set; }

        public string? ExternalReferenceId { get; set; }

        public string? RequestPayload { get; set; }

        public string? ResponsePayload { get; set; }

        public string? ErrorMessage { get; set; }

        public static ExternalLeadDeliveryResult Success(
            int? httpStatusCode,
            string? externalReferenceId,
            string? requestPayload,
            string? responsePayload)
        {
            return new ExternalLeadDeliveryResult
            {
                IsSuccess = true,
                IsRetryable = false,
                HttpStatusCode = httpStatusCode,
                ExternalReferenceId = externalReferenceId,
                RequestPayload = requestPayload,
                ResponsePayload = responsePayload
            };
        }

        public static ExternalLeadDeliveryResult Failed(
            bool isRetryable,
            int? httpStatusCode,
            string? requestPayload,
            string? responsePayload,
            string? errorMessage)
        {
            return new ExternalLeadDeliveryResult
            {
                IsSuccess = false,
                IsRetryable = isRetryable,
                HttpStatusCode = httpStatusCode,
                RequestPayload = requestPayload,
                ResponsePayload = responsePayload,
                ErrorMessage = errorMessage
            };
        }
    }
}