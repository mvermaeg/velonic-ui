using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.ExternalDeliveries;

namespace MyApp.Api.Services.Routing;

public sealed class ExclusiveLeadRoutingService
{
    private static readonly HashSet<string> AllowedPlatforms = new(StringComparer.OrdinalIgnoreCase)
    {
        "BLUEINK", "NETWORX", "Modernize", "Mili", "InsuranceTales"
    };

    private readonly MyAppDbContext _db;
    private readonly Dictionary<string, IExternalLeadProvider> _providers;
    private readonly ExclusiveRoutingOptions _options;
    private readonly ILogger<ExclusiveLeadRoutingService> _logger;

    public ExclusiveLeadRoutingService(
        MyAppDbContext db,
        IEnumerable<IExternalLeadProvider> providers,
        IOptions<ExclusiveRoutingOptions> options,
        ILogger<ExclusiveLeadRoutingService> logger)
    {
        _db = db;
        _providers = providers.ToDictionary(x => x.PlatformCode, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
        _logger = logger;
    }

    public async Task QueueAsync(Lead lead, string verticalCode, CancellationToken ct)
    {
        if (!_options.Enabled)
            return;

        var exists = await _db.LeadRoutingRuns.AnyAsync(x => x.LeadId == lead.Id, ct);
        if (exists)
            return;

        _db.LeadRoutingRuns.Add(new LeadRoutingRun
        {
            LeadId = lead.Id,
            VerticalCode = verticalCode,
            Status = "Pending",
            NextAttemptOn = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        });

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            if (!await _db.LeadRoutingRuns.AnyAsync(x => x.LeadId == lead.Id, ct))
                throw;
        }
    }

    public async Task ProcessAsync(long runId, CancellationToken ct)
    {
        var run = await _db.LeadRoutingRuns.FirstOrDefaultAsync(x => x.Id == runId, ct);
        if (run == null || run.RoutingMode != "Legacy") return;
        await using var ownership = await LeadExecutionLock.AcquireAsync(_db, run.LeadId, ct);
        if (ownership == null) return;
        await _db.Entry(run).ReloadAsync(ct);
        if (run.Status is "Completed" or "Won" or "NoMatch")
            return;

        var lead = await _db.Leads.Include(x => x.LeadType)
            .FirstOrDefaultAsync(x => x.Id == run.LeadId && !x.IsDeleted, ct);
        if (lead == null)
        {
            await FinishAsync(run, "Failed", "Lead was not found.", ct);
            return;
        }

        run.Status = "Processing";
        run.NextAttemptOn = null;
        run.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Never automatically repeat an attempt whose final outcome is unknown.
        // This favors manual review over any risk of selling the same lead twice.
        if (await _db.LeadRoutingAttempts.AnyAsync(
                x => x.LeadRoutingRunId == run.Id && x.Status == "Processing", ct))
        {
            await FinishAsync(run, "Failed", "An earlier attempt has an unknown outcome; manual review is required.", ct);
            return;
        }

        var candidates = await GetCandidatesAsync(run, lead, ct);
        if (candidates.Count == 0)
        {
            await FinishAsync(run, "NoMatch", "No eligible active routing rule matched this lead.", ct);
            return;
        }

        foreach (var rule in candidates)
        {
            var priorAttempts = await _db.LeadRoutingAttempts
                .Where(x => x.LeadRoutingRunId == run.Id && x.LeadRoutingRuleId == rule.Id)
                .OrderByDescending(x => x.AttemptNumber).ToListAsync(ct);

            if (priorAttempts.Any(x => x.Status == "Delivered"))
                return;

            var last = priorAttempts.FirstOrDefault();
            if (last != null && last.Status == "Rejected")
                continue;

            if (last?.IsRetryable == true && priorAttempts.Count < _options.MaxAttemptsPerDestination)
            {
                await AttemptAsync(run, lead, rule, priorAttempts.Count + 1, ct);
                return;
            }

            if (last?.IsRetryable == true && priorAttempts.Count >= _options.MaxAttemptsPerDestination)
                continue;

            var accepted = await AttemptAsync(run, lead, rule, priorAttempts.Count + 1, ct);
            if (accepted || run.Status == "RetryScheduled")
                return;
        }

        await FinishAsync(run, "Failed", "Every eligible destination rejected or exhausted its retries.", ct);
    }

    private async Task<List<LeadRoutingRule>> GetCandidatesAsync(LeadRoutingRun run, Lead lead, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var dayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var state = lead.State?.Trim().ToUpperInvariant();
        var postcode = lead.Postcode?.Trim();

        var rules = await _db.LeadRoutingRules.AsNoTracking()
            .Where(x => x.IsActive && x.VerticalCode == run.VerticalCode &&
                (x.State == null || x.State == "" || x.State == state) &&
                (x.Postcode == null || x.Postcode == "" || x.Postcode == postcode))
            .OrderByDescending(x => x.BidAmount).ThenBy(x => x.Priority).ThenBy(x => x.Id)
            .ToListAsync(ct);

        var eligible = new List<LeadRoutingRule>();
        var seenDestinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rule in rules)
        {
            if (rule.DestinationType == "ExternalPlatform" &&
                (string.IsNullOrWhiteSpace(rule.PlatformCode) || !AllowedPlatforms.Contains(rule.PlatformCode)))
                continue;

            if (rule.DestinationType == "InternalClient" && !rule.ClientId.HasValue)
                continue;

            var destinationKey = rule.DestinationType == "InternalClient"
                ? $"CLIENT:{rule.ClientId}"
                : $"PLATFORM:{rule.PlatformCode}";
            if (!seenDestinations.Add(destinationKey))
                continue;

            var delivered = _db.LeadRoutingAttempts.Where(x =>
                x.LeadRoutingRuleId == rule.Id && x.Status == "Delivered" && x.CompletedOn != null);
            if (rule.DailyCap is > 0 && await delivered.CountAsync(x => x.CompletedOn >= dayStart, ct) >= rule.DailyCap)
                continue;
            if (rule.MonthlyCap is > 0 && await delivered.CountAsync(x => x.CompletedOn >= monthStart, ct) >= rule.MonthlyCap)
                continue;

            eligible.Add(rule);
        }
        return eligible;
    }

    private async Task<bool> AttemptAsync(
        LeadRoutingRun run, Lead lead, LeadRoutingRule rule, int attemptNumber, CancellationToken ct)
    {
        var attempt = new LeadRoutingAttempt
        {
            LeadRoutingRunId = run.Id,
            LeadId = lead.Id,
            LeadRoutingRuleId = rule.Id,
            DestinationType = rule.DestinationType,
            ClientId = rule.ClientId,
            PlatformCode = rule.PlatformCode,
            BidAmount = rule.BidAmount,
            AttemptNumber = attemptNumber,
            Status = "Processing",
            CreatedOn = DateTime.UtcNow
        };
        _db.LeadRoutingAttempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        if (rule.DestinationType == "InternalClient")
        {
            var clientExists = await _db.Clients.AnyAsync(x => x.Id == rule.ClientId && !x.IsDeleted &&
                x.IsBuyer && x.AcceptsWebLeads && x.AccountStatus == "Active", ct);
            if (!clientExists)
            {
                await RejectAsync(attempt, "The internal client is unavailable or not an active buyer.", ct);
                return false;
            }

            var bidding = new LeadBiddingResult
            {
                LeadId = lead.Id, ClientId = rule.ClientId!.Value, BidAmount = rule.BidAmount,
                MatchReason = $"Exclusive routing rule {rule.Id}", IsWon = true, IsSold = true,
                SoldOn = DateTime.UtcNow, CreatedOn = DateTime.UtcNow
            };
            _db.LeadBiddingResults.Add(bidding);
            await _db.SaveChangesAsync(ct);
            _db.LeadDeliveries.Add(new LeadDelivery
            {
                LeadId = lead.Id, ClientId = rule.ClientId.Value, LeadBiddingResultId = bidding.Id,
                DeliveryType = "ExclusiveRouting", DeliveryStatus = "Delivered",
                DeliveredOn = DateTime.UtcNow, ResponseMessage = "Marked delivered by exclusive routing.",
                CreatedOn = DateTime.UtcNow
            });
            lead.LeadStatus = "Sold";
            lead.UpdatedOn = DateTime.UtcNow;
            await CompleteWinnerAsync(run, attempt, rule, null, ct);
            return true;
        }

        if (!_providers.TryGetValue(rule.PlatformCode!, out var provider))
        {
            await RejectAsync(attempt, $"No provider is registered for '{rule.PlatformCode}'.", ct);
            return false;
        }

        var delivery = await _db.ExternalLeadDeliveries.FirstOrDefaultAsync(
            x => x.LeadId == lead.Id && x.PlatformCode == rule.PlatformCode, ct);
        if (delivery?.Status == "Delivered")
        {
            await CompleteWinnerAsync(run, attempt, rule, delivery.ExternalReferenceId, ct);
            return true;
        }

        if (delivery == null)
        {
            delivery = new ExternalLeadDelivery
            {
                LeadId = lead.Id, PlatformCode = rule.PlatformCode!, VerticalCode = run.VerticalCode,
                Status = "ExclusiveProcessing", AttemptCount = 0,
                MaxAttempts = _options.MaxAttemptsPerDestination, CreatedOn = DateTime.UtcNow
            };
            _db.ExternalLeadDeliveries.Add(delivery);
        }
        delivery.Status = "ExclusiveProcessing"; // Existing delivery worker ignores exclusive rows.
        delivery.AttemptCount = attemptNumber;
        delivery.LastAttemptOn = DateTime.UtcNow;
        delivery.NextAttemptOn = null;
        delivery.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        ExternalLeadDeliveryResult result;
        try { result = await provider.DeliverAsync(lead, delivery, ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exclusive routing provider failed. LeadId={LeadId}, Platform={Platform}", lead.Id, rule.PlatformCode);
            result = ExternalLeadDeliveryResult.Failed(true, null, null, null, ex.Message);
        }

        delivery.HttpStatusCode = result.HttpStatusCode;
        delivery.ExternalReferenceId = result.ExternalReferenceId;
        delivery.RequestPayload = result.RequestPayload;
        delivery.ResponsePayload = result.ResponsePayload;
        delivery.ErrorMessage = result.ErrorMessage;
        delivery.UpdatedOn = DateTime.UtcNow;
        attempt.HttpStatusCode = result.HttpStatusCode;
        attempt.ExternalReferenceId = result.ExternalReferenceId;
        attempt.ErrorMessage = result.ErrorMessage;
        attempt.CompletedOn = DateTime.UtcNow;

        if (result.IsSuccess)
        {
            delivery.Status = "Delivered";
            delivery.DeliveredOn = DateTime.UtcNow;
            lead.LeadStatus = "Sold";
            lead.UpdatedOn = DateTime.UtcNow;
            await CompleteWinnerAsync(run, attempt, rule, result.ExternalReferenceId, ct);
            return true;
        }

        if (result.IsRetryable && attemptNumber < _options.MaxAttemptsPerDestination)
        {
            delivery.Status = "ExclusiveRetry"; // Existing worker intentionally ignores this status.
            attempt.Status = "RetryScheduled";
            attempt.IsRetryable = true;
            run.Status = "RetryScheduled";
            run.NextAttemptOn = DateTime.UtcNow.AddSeconds(Math.Max(15, _options.RetryDelaySeconds));
            run.ErrorMessage = result.ErrorMessage;
            run.UpdatedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return false;
        }

        delivery.Status = result.IsRetryable ? "Failed" : "Rejected";
        attempt.Status = "Rejected";
        attempt.IsRetryable = false;
        await _db.SaveChangesAsync(ct);
        return false;
    }

    private async Task CompleteWinnerAsync(
        LeadRoutingRun run, LeadRoutingAttempt attempt, LeadRoutingRule rule, string? reference, CancellationToken ct)
    {
        attempt.Status = "Delivered";
        attempt.IsRetryable = false;
        attempt.ExternalReferenceId ??= reference;
        attempt.CompletedOn = DateTime.UtcNow;
        run.Status = "Completed";
        run.WinnerType = rule.DestinationType;
        run.WinnerClientId = rule.ClientId;
        run.WinnerPlatformCode = rule.PlatformCode;
        run.WinningBidAmount = rule.BidAmount;
        run.CompletedOn = DateTime.UtcNow;
        run.NextAttemptOn = null;
        run.ErrorMessage = null;
        run.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task RejectAsync(LeadRoutingAttempt attempt, string error, CancellationToken ct)
    {
        attempt.Status = "Rejected";
        attempt.IsRetryable = false;
        attempt.ErrorMessage = error;
        attempt.CompletedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    private async Task FinishAsync(LeadRoutingRun run, string status, string error, CancellationToken ct)
    {
        run.Status = status;
        run.ErrorMessage = error;
        run.CompletedOn = DateTime.UtcNow;
        run.NextAttemptOn = null;
        run.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}
