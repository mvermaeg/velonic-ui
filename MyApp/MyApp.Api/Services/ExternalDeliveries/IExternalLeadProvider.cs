using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries
{
    public interface IExternalLeadProvider
    {
        string PlatformCode { get; }

        Task<ExternalLeadDeliveryResult> DeliverAsync(
            Lead lead,
            ExternalLeadDelivery delivery,
            CancellationToken cancellationToken = default);
    }
}