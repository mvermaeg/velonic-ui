using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;

namespace MyApp.Api.Services.Routing;

public interface ILeadRoutingQueue
{
    Task QueueForLeadAsync(long leadId, CancellationToken ct = default);
}

public interface ILegacyLeadCompletion
{
    Task CompleteAsync(long leadId, CancellationToken ct);
}

// Website and Meta completion use the same validated options as the workers.
// IsTest controls provider endpoints, never the choice of routing system.
public sealed class CompletedLeadRouter(ILeadRoutingQueue queue, ILegacyLeadCompletion legacy,
    IOptions<ExternalLeadAuctionOptions> auction, IOptions<ExclusiveRoutingOptions> exclusive)
{
    public async Task CompleteAsync(Lead lead, CancellationToken ct = default)
    {
        if (!lead.IsCompleted || lead.IsDeleted) return;
        if (auction.Value.Enabled)
        {
            await queue.QueueForLeadAsync(lead.Id, ct);
            return;
        }
        if (!lead.IsTest) await legacy.CompleteAsync(lead.Id, ct);
        else if (exclusive.Value.Enabled) await queue.QueueForLeadAsync(lead.Id, ct);
    }
}

public sealed class LegacyLeadCompletion(MyAppDbContext db, LeadBiddingService bidding,
    LeadDeliveryService delivery, ILeadRoutingQueue queue) : ILegacyLeadCompletion
{
    public async Task CompleteAsync(long leadId, CancellationToken ct)
    {
        await bidding.RunForLeadAsync(leadId);
        await queue.QueueForLeadAsync(leadId, ct);
        var winner = await db.LeadBiddingResults.Where(x => x.LeadId == leadId && x.IsWon && !x.IsSold)
            .OrderByDescending(x => x.BidAmount).FirstOrDefaultAsync(ct);
        if (winner != null) await delivery.CreateDeliveryFromBiddingResultAsync(winner.Id);
    }
}

public interface ILeadExecutionLocker
{
    Task<IAsyncDisposable?> AcquireAsync(MyAppDbContext db, long leadId, CancellationToken ct);
}

public sealed class SqlLeadExecutionLocker : ILeadExecutionLocker
{
    public Task<IAsyncDisposable?> AcquireAsync(MyAppDbContext db, long leadId, CancellationToken ct) =>
        LeadExecutionLock.AcquireAsync(db, leadId, ct);
}
