using Microsoft.Extensions.Options;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Routing;

public sealed class ExternalAuctionEngine(IAuctionStore store, IAuctionGateway gateway,
    IThumbtackAuctionGateway thumbtack, IOptions<ExternalLeadAuctionOptions> options)
{
    private readonly ExternalLeadAuctionOptions _options = options.Value;
    public static bool Terminal(string status) => status is "Won" or "NoBid" or "Failed" or "Cancelled" or "ThumbtackSelected" or "ThumbtackFailed" or "ReconciliationRequired";
    public async Task ProcessAsync(long id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var initial = await store.LoadAsync(id, ct);
        if (initial == null) return;
        await using var ownership = await store.LockAsync(initial.Lead.Id, ct);
        if (ownership == null) return;
        var w = await store.LoadAsync(id, ct);
        if (w == null || Terminal(w.Run.Status)) return;
        if (w.Run.WinnerAttemptId != null && w.Run.Status == "Posting") { await PostWinner(w, ct); return; }
        if (!w.Lead.IsCompleted || w.Lead.IsDeleted || !w.Lead.IsTest && (w.Lead.IsSuspicious || w.Lead.IsInvalidEmail || w.Lead.FraudScore >= 80))
        { await Finish(w, "Cancelled", "Lead is no longer eligible.", ct); return; }

        if (w.Run.WinnerAttemptId != null) { await PostWinner(w, ct); return; }
        if (w.Run.Status is "Pending" or "CheckingThumbtack")
        {
            if (w.Run.ThumbtackSearchStartedOn != null)
            {
                w.Run.WinnerPlatformCode = "THUMBTACK";
                w.Run.WinnerType = "Recommendations";
                await Finish(w, "ThumbtackFailed", "Thumbtack search outcome unknown after interruption; search and external routing suppressed for manual review.", ct);
                return;
            }
            w.Run.Status = "CheckingThumbtack";
            await store.SaveAsync(w, ct);
            bool covered;
            try { covered = w.Run.WinnerPlatformCode == "THUMBTACK" || await thumbtack.IsCoveredAsync(w, ct); }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch
            {
                await Finish(w, "Failed", "Coverage could not be confirmed; no Thumbtack lookup or external ping was sent. Manual review required.", ct);
                return;
            }
            if (covered)
            {
                // Durable exclusive ownership before any remote lookup. Availability and
                // legacy FallThrough configuration can never change this decision.
                w.Run.WinnerPlatformCode = "THUMBTACK"; w.Run.WinnerType = "Recommendations";
                w.Run.ThumbtackEligibility = "Covered service + ZIP; Thumbtack-only.";
                await store.SaveAsync(w, ct);
                try
                {
                    var check = await thumbtack.CheckAsync(w, ct);
                    w.Run.ThumbtackEligibility = "Covered service + ZIP; Thumbtack-only. " + check.Reason;
                    w.Run.ThumbtackSearchId = check.SearchId;
                    w.Run.ThumbtackResponseJson = check.Response;
                    await Finish(w, check.Eligible ? "ThumbtackSelected" : "ThumbtackFailed",
                        check.Eligible ? null : check.Reason, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch
                {
                    await Finish(w, "ThumbtackFailed", "Thumbtack lookup failed or timed out; external routing prohibited. Manual reconciliation required before retry.", ct);
                }
                return;
            }
            w.Run.ThumbtackEligibility = "No imported service + ZIP coverage; external auction only.";
            w.Run.Status = "Pinging";
            await store.SaveAsync(w, ct);
        }
        if (w.Run.Status == "Pinging")
        {
            var rules = await store.RulesAsync(w, ct);
            foreach (var rule in rules.GroupBy(x => x.PlatformCode).Select(x => x.First()))
            {
                var supported = rule.PlatformCode != null && gateway.Enabled(rule.PlatformCode, w.Run.VerticalCode);
                w.Attempts.Add(new LeadRoutingAttempt { LeadRoutingRunId = w.Run.Id, LeadId = w.Lead.Id,
                    LeadRoutingRuleId = rule.Id, DestinationType = "ExternalPlatform", PlatformCode = rule.PlatformCode,
                AttemptNumber = 1, CreatedOn = DateTime.UtcNow, Status = supported ? "PingStarted" : "Excluded",
                    PingRequestAudit = supported ? AuctionAudit.Request(rule.PlatformCode!, "ping-intent", w.Lead.Id) : null,
                    ErrorMessage = supported ? null : "Provider disabled, unsupported, or missing a monetary bid contract." });
            }
            w.Run.AuctionStartedOn = DateTime.UtcNow;
            w.Run.AuctionClosesOn = w.Run.AuctionStartedOn.Value.AddSeconds(Math.Clamp(_options.AuctionWindowSeconds, 1, 300));
            w.Run.Status = "CollectingBids";
            // Durable send intent before any ping: recovery never blindly sends it again.
            await store.SaveAsync(w, ct);
            var facts = AuctionFacts.From(w.Lead, w.Run.VerticalCode);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var remaining = w.Run.AuctionClosesOn.Value - DateTime.UtcNow;
            deadline.CancelAfter(remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero);
            var pending = w.Attempts.Where(x => x.Status == "PingStarted")
                .Select(a => Receive(a, facts, w.Run.AuctionClosesOn.Value, deadline.Token)).ToList();
            while (pending.Count > 0)
            {
                var done = await Task.WhenAny(pending);
                pending.Remove(done);
                var received = await done;
                ct.ThrowIfCancellationRequested();
                var a = received.Attempt;
                a.BidReceivedOn = received.ReceivedOn; a.ResponseDurationMilliseconds = received.Duration;
                a.OfferedBidAmount = received.Offer.Amount; a.BidExpiresOn = received.Offer.ExpiresOn;
                a.PingRequestAudit = received.Offer.RequestAudit; a.PingResponseAudit = received.Offer.ResponseAudit;
                a.PingReferenceId = string.IsNullOrWhiteSpace(received.Offer.Reference) ? null : gateway.Protect(received.Offer.Reference);
                a.PostContext = received.Offer.PostContext == null ? null : gateway.Protect(received.Offer.PostContext);
                var rejected = received.Timeout ? "TimedOut" : AuctionPolicy.Reject(received.Offer, received.ReceivedOn,
                    w.Run.AuctionClosesOn.Value, DateTime.UtcNow);
                a.Status = rejected ?? "BidAccepted"; a.ErrorMessage = received.Offer.Error ?? rejected;
                await store.SaveAsync(w, ct);
            }
        }
        // Restart during collection: retain completed bids and fence ambiguous sends.
        foreach (var a in w.Attempts.Where(x => x.Status == "PingStarted"))
        { a.Status = "Interrupted"; a.ErrorMessage = "Ping outcome unknown after restart; ping not repeated."; }
        await store.SaveAsync(w, ct);
        while (true)
        {
            var winner = AuctionPolicy.Winner(w.Attempts, w.Run.AuctionClosesOn ?? DateTime.MinValue, DateTime.UtcNow);
            if (winner == null) { await Finish(w, "NoBid", "No valid available bids.", ct); return; }
            if (await store.LockWinnerAsync(w, winner, ct)) break;
            winner.Status = "CapReached";
            await store.SaveAsync(w, ct);
        }
        await PostWinner(w, ct);
    }
    private async Task PostWinner(AuctionWork w, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var a = w.Attempts.Single(x => x.Id == w.Run.WinnerAttemptId && x.IsWinner);
        if (w.Run.PostCompletedOn != null) return;
        if (w.Run.Status == "Posting" && !gateway.Idempotent(a.PlatformCode!))
        {
            a.Status = "PostUnknown"; a.ErrorMessage = "Post outcome unknown after restart; reconcile with the locked winner.";
            await Finish(w, "ReconciliationRequired", "Post outcome unknown after restart. Reconcile with the locked winner; automatic replay is unsafe.", ct); return;
        }
        if (w.Run.NextAttemptOn > DateTime.UtcNow) return;
        if (!gateway.Enabled(a.PlatformCode!, w.Run.VerticalCode))
        { await Finish(w, "Failed", "Locked winner is disabled. No fallback permitted.", ct); return; }
        if (a.BidExpiresOn <= DateTime.UtcNow || a.PostAttempts >= Math.Max(1, _options.MaxWinnerPostAttempts))
        { await Finish(w, "Failed", "Winner offer expired or retry limit reached.", ct); return; }
        w.Run.Status = "Posting"; w.Run.NextAttemptOn = null;
        a.PostStartedOn = DateTime.UtcNow; a.PostAttempts++;
        a.PostRequestAudit = AuctionAudit.Request(a.PlatformCode!, "post", w.Lead.Id);
        await store.SaveAsync(w, ct); // Write-ahead fence for crashes/timeouts.
        AuctionPost result;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.PostTimeoutSeconds, 1, 120)));
        try
        {
            result = await gateway.PostAsync(a.PlatformCode!, w, gateway.Unprotect(a.PingReferenceId!),
                a.PostContext == null ? null : gateway.Unprotect(a.PostContext), $"HOMEYY-AUCTION-{w.Run.Id}", timeout.Token)
                .WaitAsync(timeout.Token);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { result = new(false, gateway.Idempotent(a.PlatformCode!), true, null, a.PostRequestAudit, "{}", "Post outcome unknown; reconcile with provider."); }
        a.PostRequestAudit = result.RequestAudit; a.PostResponseAudit = result.ResponseAudit; a.ErrorMessage = result.Error; a.HttpStatusCode = result.HttpStatus;
        a.ExternalReferenceId = result.DeliveryId; a.Status = result.Accepted ? "Delivered" : result.OutcomeUnknown ? "PostUnknown" : "PostFailed";
        a.IsRetryable = AuctionPolicy.CanRetry(result, gateway.Idempotent(a.PlatformCode!));
        if (result.Accepted)
        { w.Run.Status = "Won"; w.Run.PostCompletedOn = DateTime.UtcNow; w.Run.CompletedOn = DateTime.UtcNow; a.CompletedOn = DateTime.UtcNow; }
        else if (AuctionPolicy.CanRetry(result, gateway.Idempotent(a.PlatformCode!)) && a.PostAttempts < _options.MaxWinnerPostAttempts)
        { w.Run.Status = "WinnerSelected"; w.Run.NextAttemptOn = DateTime.UtcNow.AddSeconds(Math.Max(1, _options.RetryDelaySeconds)); }
        else { w.Run.Status = result.OutcomeUnknown ? "ReconciliationRequired" : "Failed"; w.Run.CompletedOn = DateTime.UtcNow; }
        w.Run.ErrorMessage = result.Error;
        await store.RecordDeliveryAsync(w, a, result, ct);
    }
    public async Task<bool> ReconcileAsync(long id, bool accepted, string reference, string actor, CancellationToken ct)
    {
        var initial = await store.LoadAsync(id, ct);
        if (initial == null) return false;
        await using var ownership = await store.LockAsync(initial.Lead.Id, ct);
        if (ownership == null) return false;
        var w = await store.LoadAsync(id, ct);
        if (w == null || w.Run.Status != "ReconciliationRequired" || w.Run.ReconciledOn != null) return false;
        var a = w.Attempts.Single(x => x.Id == w.Run.WinnerAttemptId && x.IsWinner);
        w.Run.ReconciledOn = DateTime.UtcNow; w.Run.ReconciledBy = actor;
        w.Run.ReconciliationReference = reference; w.Run.ReconciliationOutcome = accepted ? "Accepted" : "NotAccepted";
        w.Run.Status = accepted ? "Won" : "Failed"; w.Run.CompletedOn = DateTime.UtcNow;
        w.Run.PostCompletedOn = accepted ? DateTime.UtcNow : null; w.Run.NextAttemptOn = null;
        w.Run.ErrorMessage = accepted ? null : "Provider confirmed no acceptance; closed without replay or fallback.";
        a.Status = accepted ? "Delivered" : "PostFailed"; a.IsRetryable = false; a.CompletedOn = DateTime.UtcNow;
        a.ErrorMessage = w.Run.ErrorMessage;
        await store.RecordDeliveryAsync(w, a, new(accepted, false, false, a.ExternalReferenceId,
            a.PostRequestAudit ?? "{}", a.PostResponseAudit ?? "{}", w.Run.ErrorMessage), ct);
        return true;
    }
    private async Task Finish(AuctionWork w, string status, string? error, CancellationToken ct)
    { w.Run.Status = status; w.Run.ErrorMessage = error; w.Run.CompletedOn = DateTime.UtcNow; await store.SaveAsync(w, ct); }
    private async Task<Received> Receive(LeadRoutingAttempt a, AuctionFacts facts, DateTime closes, CancellationToken ct)
    {
        var start = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var offer = await gateway.PingAsync(a.PlatformCode!, facts, ct).WaitAsync(ct);
            return new(a, offer, DateTime.UtcNow, start.ElapsedMilliseconds, false);
        }
        catch (OperationCanceledException)
        { return new(a, new(false, null, null, null, AuctionAudit.Request(a.PlatformCode!, "ping", facts.LeadId), "{}", "Auction deadline reached."), DateTime.UtcNow, start.ElapsedMilliseconds, true); }
        catch
        { return new(a, new(false, null, null, null, AuctionAudit.Request(a.PlatformCode!, "ping", facts.LeadId), "{}", "Provider ping failed."), DateTime.UtcNow, start.ElapsedMilliseconds, false); }
    }
    private sealed record Received(LeadRoutingAttempt Attempt, AuctionOffer Offer, DateTime ReceivedOn, long Duration, bool Timeout);
}
