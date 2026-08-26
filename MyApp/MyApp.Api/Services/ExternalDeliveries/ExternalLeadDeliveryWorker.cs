using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Services.ExternalDeliveries
{
    public class ExternalLeadDeliveryWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExternalLeadDeliveryWorker> _logger;

        public ExternalLeadDeliveryWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<ExternalLeadDeliveryWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(10),
                stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "External lead delivery worker failed.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(15),
                    stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<MyAppDbContext>();

            var providers = scope.ServiceProvider
                .GetServices<IExternalLeadProvider>()
                .ToDictionary(
                    x => x.PlatformCode,
                    StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var staleProcessingTime = now.AddMinutes(-10);

            var deliveries = await db.ExternalLeadDeliveries
                .Include(x => x.Lead)
                    .ThenInclude(x => x.LeadType)
                .Where(x =>
                    x.AttemptCount < x.MaxAttempts &&
                    (
                        (
                            x.Status == "Pending" ||
                            x.Status == "RetryScheduled"
                        ) &&
                        (
                            x.NextAttemptOn == null ||
                            x.NextAttemptOn <= now
                        )
                    ||
                        (
                            x.Status == "Processing" &&
                            x.LastAttemptOn < staleProcessingTime
                        )
                    ))
                .OrderBy(x => x.CreatedOn)
                .Take(20)
                .ToListAsync(cancellationToken);

            foreach (var delivery in deliveries)
            {
                await ProcessDeliveryAsync(
                    db,
                    providers,
                    delivery,
                    cancellationToken);
            }
        }

        private async Task ProcessDeliveryAsync(
            MyAppDbContext db,
            Dictionary<string, IExternalLeadProvider> providers,
            Data.Entities.ExternalLeadDelivery delivery,
            CancellationToken cancellationToken)
        {
            if (!providers.TryGetValue(
                    delivery.PlatformCode,
                    out var provider))
            {
                delivery.Status = "Failed";
                delivery.ErrorMessage =
                    $"No provider is registered for platform '{delivery.PlatformCode}'.";
                delivery.UpdatedOn = DateTime.UtcNow;

                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            delivery.Status = "Processing";
            delivery.AttemptCount++;
            delivery.LastAttemptOn = DateTime.UtcNow;
            delivery.NextAttemptOn = null;
            delivery.ErrorMessage = null;
            delivery.UpdatedOn = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            ExternalLeadDeliveryResult result;

            try
            {
                result = await provider.DeliverAsync(
                    delivery.Lead,
                    delivery,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result = ExternalLeadDeliveryResult.Failed(
                    isRetryable: true,
                    httpStatusCode: null,
                    requestPayload: null,
                    responsePayload: null,
                    errorMessage: ex.Message);
            }

            delivery.HttpStatusCode = result.HttpStatusCode;
            delivery.ExternalReferenceId =
                result.ExternalReferenceId;
            delivery.RequestPayload = result.RequestPayload;
            delivery.ResponsePayload = result.ResponsePayload;
            delivery.ErrorMessage = result.ErrorMessage;
            delivery.UpdatedOn = DateTime.UtcNow;

            if (result.IsSuccess)
            {
                delivery.Status = "Delivered";
                delivery.DeliveredOn = DateTime.UtcNow;
                delivery.NextAttemptOn = null;
            }
            else if (
                result.IsRetryable &&
                delivery.AttemptCount < delivery.MaxAttempts)
            {
                delivery.Status = "RetryScheduled";
                delivery.NextAttemptOn = CalculateNextAttempt(
                    delivery.AttemptCount);
            }
            else
            {
                delivery.Status =
                    result.IsRetryable
                        ? "Failed"
                        : "Rejected";

                delivery.NextAttemptOn = null;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static DateTime CalculateNextAttempt(int attemptCount)
        {
            var delayMinutes = attemptCount switch
            {
                1 => 1,
                2 => 5,
                3 => 15,
                4 => 30,
                _ => 60
            };

            return DateTime.UtcNow.AddMinutes(delayMinutes);
        }
    }
}