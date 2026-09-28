using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;

namespace MyApp.Api.Services.Routing;

public sealed class ExclusiveLeadRoutingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<ExclusiveRoutingOptions> _options;
    private readonly ILogger<ExclusiveLeadRoutingWorker> _logger;

    public ExclusiveLeadRoutingWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<ExclusiveRoutingOptions> options,
        ILogger<ExclusiveLeadRoutingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_options.CurrentValue.Enabled)
                    await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Exclusive lead routing worker failed."); }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, _options.CurrentValue.PollSeconds)), stoppingToken);
        }
    }

        private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        if (scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ExternalLeadAuctionOptions>>().Value.Enabled) return;
        var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ExclusiveLeadRoutingService>();
        var now = DateTime.UtcNow;
        var stale = now.AddMinutes(-10);
        var ids = await db.LeadRoutingRuns.AsNoTracking()
            .Where(x => x.RoutingMode == "Legacy" && ((x.Status == "Pending" || x.Status == "RetryScheduled") &&
                (x.NextAttemptOn == null || x.NextAttemptOn <= now) ||
                (x.Status == "Processing" && x.UpdatedOn < stale)))
            .OrderBy(x => x.CreatedOn).Select(x => x.Id).Take(10).ToListAsync(ct);

        foreach (var id in ids)
            await service.ProcessAsync(id, ct);
    }
}
