using System.Data;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Routing;

public sealed record AuctionWork(LeadRoutingRun Run, Lead Lead, List<LeadRoutingAttempt> Attempts);
public interface IAuctionStore
{
    Task<AuctionWork?> LoadAsync(long id, CancellationToken ct);
    Task<IAsyncDisposable?> LockAsync(long leadId, CancellationToken ct);
    Task<List<LeadRoutingRule>> RulesAsync(AuctionWork work, CancellationToken ct);
    Task SaveAsync(AuctionWork work, CancellationToken ct);
    Task<bool> LockWinnerAsync(AuctionWork work, LeadRoutingAttempt attempt, CancellationToken ct);
    Task RecordDeliveryAsync(AuctionWork work, LeadRoutingAttempt attempt, AuctionPost result, CancellationToken ct);
}

public sealed class LeadExecutionLock : IAsyncDisposable
{
    private readonly MyAppDbContext _db;
    private readonly string _resource;
    private LeadExecutionLock(MyAppDbContext db, string resource) { _db = db; _resource = resource; }
    public static async Task<IAsyncDisposable?> AcquireAsync(MyAppDbContext db, long leadId, CancellationToken ct)
        => await AcquireResourceAsync(db, $"Homeyy.ExternalLead.{leadId}", ct);
    public static async Task<IAsyncDisposable?> AcquireResourceAsync(MyAppDbContext db, string resource, CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@resource, @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=0; SELECT @r;";
        var parameter = command.CreateParameter(); parameter.ParameterName = "@resource"; parameter.Value = resource; command.Parameters.Add(parameter);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(ct));
        if (result >= 0) return new LeadExecutionLock(db, resource);
        await db.Database.CloseConnectionAsync();
        return null;
    }
    public async ValueTask DisposeAsync()
    {
        try
        {
            using var command = _db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC sys.sp_releaseapplock @Resource=@resource, @LockOwner='Session';";
            var parameter = command.CreateParameter(); parameter.ParameterName = "@resource"; parameter.Value = _resource; command.Parameters.Add(parameter);
            if (command.Connection!.State == ConnectionState.Open) await command.ExecuteNonQueryAsync();
        }
        finally { await _db.Database.CloseConnectionAsync(); }
    }
}

public sealed class SqlAuctionStore(MyAppDbContext db, IAuctionGateway gateway) : IAuctionStore
{
    public Task<IAsyncDisposable?> LockAsync(long leadId, CancellationToken ct) => LeadExecutionLock.AcquireAsync(db, leadId, ct);
    public async Task<AuctionWork?> LoadAsync(long id, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        var run = await db.LeadRoutingRuns.Include(x => x.Lead).ThenInclude(x => x.LeadType)
            .Include(x => x.LeadRoutingAttempts).FirstOrDefaultAsync(x => x.Id == id && x.RoutingMode == "Auction", ct);
        return run == null ? null : new(run, run.Lead, run.LeadRoutingAttempts.ToList());
    }
    public async Task<List<LeadRoutingRule>> RulesAsync(AuctionWork w, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        // Materialize a default rule for each configured adapter that has no rule.
        // Existing disabled rules remain deliberate opt-outs; existing caps survive.
        await using (var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct))
        {
            var existing = await db.LeadRoutingRules.FromSqlInterpolated($"SELECT * FROM VelonicDBUser.LeadRoutingRules WITH (UPDLOCK,HOLDLOCK) WHERE DestinationType='ExternalPlatform' AND VerticalCode={w.Run.VerticalCode}").ToListAsync(ct);
            foreach (var platform in AuctionRulePolicy.MissingProviders(existing, w.Run.VerticalCode, gateway))
                db.LeadRoutingRules.Add(new LeadRoutingRule { DestinationType = "ExternalPlatform", PlatformCode = platform,
                    VerticalCode = w.Run.VerticalCode, IsActive = true, Priority = 100, CreatedOn = now, UpdatedOn = now });
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        var rules = await db.LeadRoutingRules.AsNoTracking().Where(AuctionRulePolicy.Eligible(w.Run.VerticalCode, now))
            .OrderBy(x => x.Priority).ThenBy(x => x.Id).ToListAsync(ct);
        var month = new DateTime(now.Year, now.Month, 1);
        var eligible = new List<LeadRoutingRule>();
        foreach (var rule in rules)
        {
            var reservations = db.LeadRoutingAttempts.Where(x => (x.IsWinner || x.Status == "Delivered") && x.LeadRoutingRuleId == rule.Id);
            if (rule.DailyCap is > 0 && await reservations.CountAsync(x => (x.LeadRoutingRun.WinnerLockedOn ?? x.CompletedOn) >= now.Date, ct) >= rule.DailyCap ||
                rule.MonthlyCap is > 0 && await reservations.CountAsync(x => (x.LeadRoutingRun.WinnerLockedOn ?? x.CompletedOn) >= month, ct) >= rule.MonthlyCap) continue;
            eligible.Add(rule);
        }
        return eligible;
    }
    public async Task SaveAsync(AuctionWork work, CancellationToken ct)
    {
        foreach (var a in work.Attempts.Where(x => x.Id == 0))
            if (db.Entry(a).State == EntityState.Detached) db.LeadRoutingAttempts.Add(a);
        work.Run.UpdatedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> LockWinnerAsync(AuctionWork work, LeadRoutingAttempt attempt, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var rule = await db.LeadRoutingRules.FromSqlInterpolated($"SELECT * FROM VelonicDBUser.LeadRoutingRules WITH (UPDLOCK,HOLDLOCK) WHERE Id={attempt.LeadRoutingRuleId}").AsNoTracking().SingleAsync(ct);
        var now = DateTime.UtcNow;
        var month = new DateTime(now.Year, now.Month, 1);
        var reservations = db.LeadRoutingAttempts.Where(x => (x.IsWinner || x.Status == "Delivered") && x.LeadRoutingRuleId == rule.Id && x.LeadRoutingRunId != work.Run.Id);
        if (!AuctionRulePolicy.Eligible(work.Run.VerticalCode, now).Compile()(rule) || rule.PlatformCode != attempt.PlatformCode ||
            rule.DailyCap is > 0 && await reservations.CountAsync(x => (x.LeadRoutingRun.WinnerLockedOn ?? x.CompletedOn) >= now.Date, ct) >= rule.DailyCap ||
            rule.MonthlyCap is > 0 && await reservations.CountAsync(x => (x.LeadRoutingRun.WinnerLockedOn ?? x.CompletedOn) >= month, ct) >= rule.MonthlyCap)
        { await tx.RollbackAsync(ct); return false; }
        if (work.Run.WinnerAttemptId != null) return work.Run.WinnerAttemptId == attempt.Id;
        attempt.IsWinner = true;
        work.Run.WinnerAttemptId = attempt.Id;
        work.Run.WinnerPlatformCode = attempt.PlatformCode;
        work.Run.WinnerType = "ExternalPlatform";
        work.Run.WinningBidAmount = attempt.OfferedBidAmount;
        work.Run.WinnerSelectedOn = now; work.Run.WinnerLockedOn = now;
        work.Run.Status = "WinnerSelected";
        await SaveAsync(work, ct);
        await tx.CommitAsync(ct);
        return true;
    }
    public async Task RecordDeliveryAsync(AuctionWork w, LeadRoutingAttempt a, AuctionPost result, CancellationToken ct)
    {
        var delivery = await db.ExternalLeadDeliveries.SingleOrDefaultAsync(x => x.LeadId == w.Lead.Id && x.PlatformCode == a.PlatformCode, ct);
        if (delivery == null)
        {
            delivery = new ExternalLeadDelivery { LeadId = w.Lead.Id, PlatformCode = a.PlatformCode!, VerticalCode = w.Run.VerticalCode,
                CreatedOn = DateTime.UtcNow, MaxAttempts = 1 };
            db.ExternalLeadDeliveries.Add(delivery);
        }
        delivery.Status = result.Accepted ? "Delivered" : "AuctionFailed";
        delivery.AttemptCount = a.PostAttempts; delivery.LastAttemptOn = a.PostStartedOn;
        delivery.DeliveredOn = result.Accepted ? DateTime.UtcNow : null;
        delivery.ExternalReferenceId = result.DeliveryId; delivery.RequestPayload = result.RequestAudit;
        delivery.ResponsePayload = result.ResponseAudit; delivery.HttpStatusCode = result.HttpStatus;
        delivery.ErrorMessage = result.Error; delivery.UpdatedOn = DateTime.UtcNow;
        if (result.Accepted) { w.Lead.LeadStatus = "Sold"; w.Lead.UpdatedOn = DateTime.UtcNow; }
        await SaveAsync(w, ct);
    }
}
