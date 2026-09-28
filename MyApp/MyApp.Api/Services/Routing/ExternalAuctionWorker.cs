using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
namespace MyApp.Api.Services.Routing;

public sealed class ExternalAuctionWorker(IServiceScopeFactory scopes, IOptions<ExternalLeadAuctionOptions> options,
    ILogger<ExternalAuctionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (options.Value.Enabled)
                {
                    using var scope = scopes.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
                    var now = DateTime.UtcNow;
                    var ids = await db.LeadRoutingRuns.AsNoTracking().Where(x => x.RoutingMode == "Auction" &&
                        (x.Status == "Pending" || x.Status == "CheckingThumbtack" || x.Status == "Pinging" ||
                         x.Status == "CollectingBids" || x.Status == "WinnerSelected" || x.Status == "Posting") &&
                        (x.NextAttemptOn == null || x.NextAttemptOn <= now)).OrderBy(x => x.CreatedOn).Select(x => x.Id).Take(20).ToListAsync(stoppingToken);
                    foreach (var id in ids)
                    {
                        using var workScope = scopes.CreateScope();
                        try { await workScope.ServiceProvider.GetRequiredService<ExternalAuctionEngine>().ProcessAsync(id, stoppingToken); }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                        catch { logger.LogError("Auction run {RunId} interrupted; durable state retained for recovery.", id); }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch { logger.LogError("Auction worker could not read pending work."); await Task.Delay(5000, stoppingToken); }
        }
    }
}
