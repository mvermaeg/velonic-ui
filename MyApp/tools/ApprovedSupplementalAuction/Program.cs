using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.ExternalDeliveries.BlueInk;
using MyApp.Api.Services.ExternalDeliveries.Mili;
using MyApp.Api.Services.Routing;

// Intentionally restricted to the ten IDs approved by the owner on 2026-10-11.
// This is a separate maintenance round, never a reset of the original winner.
var ids = new long[] { 131, 133, 134, 135, 136, 137, 138, 143, 145, 148 };
if (!args.Contains("--execute-approved-unmatched-20261011")) {
    Console.WriteLine("Dry run: approved IDs " + string.Join(',', ids) + "; NETWORX excluded; one additional winner only."); return;
}
var api = Path.GetFullPath("MyApp.Api");
var config = new ConfigurationBuilder().SetBasePath(api).AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Production.json").Build();
var backup = new ConfigurationBuilder().SetBasePath(api).AddJsonFile("appsettings.backup-20260821-235015.json").Build();
var connection = backup.GetConnectionString("DefaultConnection")!;
var target = new SqlConnectionStringBuilder(connection);
if (target.DataSource != "52.54.2.36,1433" || target.InitialCatalog != "VelonicDB") throw new InvalidOperationException("Unexpected target.");
target.ConnectTimeout = 8; connection = target.ConnectionString;
var blueOptions = config.GetSection(BlueInkOptions.SectionName).Get<BlueInkOptions>()!;
var miliOptions = config.GetSection(MiliOptions.SectionName).Get<MiliOptions>()!;
if (blueOptions.UseTestEndpoints || miliOptions.UseTestMode || miliOptions.LeadType != "Shared") throw new InvalidOperationException("Approved round requires production endpoints and configured Mili Shared type.");
var keyDirectory = Path.Combine(AppContext.BaseDirectory, "supplemental-key-ring");
Directory.CreateDirectory(keyDirectory);
var protector = DataProtectionProvider.Create(new DirectoryInfo(keyDirectory), b => b.SetApplicationName("Homeyy.ApprovedSupplementalAuction"))
    .CreateProtector("unmatched-20261011");
using var blueHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
using var miliHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
var blue = new BlueInkLeadProvider(blueHttp, Options.Create(blueOptions), NullLogger<BlueInkLeadProvider>.Instance);
var mili = new MiliLeadProvider(miliHttp, Options.Create(miliOptions), new MiliRateLimiter(), NullLogger<MiliLeadProvider>.Instance);
Console.WriteLine("Target 52.54.2.36,1433 / VelonicDB. Original NETWORX records preserved. Credentials redacted.");
foreach (var id in ids) {
    try { await Process(id); }
    catch { Console.WriteLine($"Lead {id}: interrupted or failed validation; durable state retained. No automatic replay."); }
}

async Task Process(long id) {
    await using var c = new SqlConnection(connection); await c.OpenAsync();
    var acquired = await Scalar(c, "DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@resource,@LockMode='Exclusive',@LockOwner='Session',@LockTimeout=0; SELECT @r;", ("@resource", $"Homeyy.ExternalLead.{id}"));
    if (Convert.ToInt32(acquired) < 0) { Console.WriteLine($"Lead {id}: busy; skipped."); return; }
    await using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer(connection).Options);
    var lead = await db.Leads.Include(l => l.LeadType).SingleAsync(l => l.Id == id);
    var original = await db.LeadRoutingRuns.SingleAsync(r => r.LeadId == id);
    if (lead.IsDeleted || lead.IsTest || !lead.IsCompleted || lead.IsInvalidEmail || original.Status != "Won" || original.WinnerPlatformCode != "NETWORX" || original.PostCompletedOn == null)
        throw new InvalidOperationException("Original ownership or eligibility changed.");
    if (await MyApp.Api.Services.Thumbtack.ThumbtackCoverageLookup.HasCoverageAsync(db, "Roofing", lead.Postcode, default))
        throw new InvalidOperationException("Thumbtack coverage prevents external resale.");
    var previous = await Scalar(c, "SELECT Status FROM VelonicDBUser.LeadSupplementalAuctions WHERE LeadId=@id;", ("@id", id));
    if (previous != null && previous != DBNull.Value) {
        if ((string)previous == "Posting") await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='ReconciliationRequired',ErrorCategory='Interrupted post; acceptance unconfirmed; no replay.',UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id AND Status='Posting';", ("@id", id));
        Console.WriteLine($"Lead {id}: existing supplemental round {previous}; not repeated."); return;
    }
    if (await db.ExternalLeadDeliveries.AnyAsync(d => d.LeadId == id && d.PlatformCode != "NETWORX" &&
            (d.DeliveredOn != null || d.Status == "Delivered" || d.Status == "AuctionFailed")) ||
        await db.LeadRoutingAttempts.AnyAsync(a => a.LeadId == id && a.PlatformCode != "NETWORX" &&
            (a.PostAttempts > 0 || a.IsWinner || a.Status == "PostUnknown")))
        throw new InvalidOperationException("Another provider has a previous post or uncertain outcome.");
    if (await db.LeadDeliveries.AnyAsync(d => d.LeadId == id)) throw new InvalidOperationException("Internal delivery prevents supplemental routing.");
    await Exec(c, "INSERT VelonicDBUser.LeadSupplementalAuctions(LeadId,BatchCode,OriginalRunId,Status,CreatedOn,UpdatedOn) VALUES(@id,'unmatched-20261011',@run,'Pinging',SYSUTCDATETIME(),SYSUTCDATETIME()); INSERT VelonicDBUser.LeadStatusHistory(LeadId,OldStatus,NewStatus,ChangedOn,Remarks) VALUES(@id,@status,@status,SYSUTCDATETIME(),'User authorized one additional winner for unmatched-20261011; NETWORX excluded. Risk score preserved.');", ("@id", id), ("@run", original.Id), ("@status", lead.LeadStatus));
    var rules = await db.LeadRoutingRules.AsNoTracking().Where(AuctionRulePolicy.Eligible("Roofing", DateTime.UtcNow)).ToListAsync();
    bool Allows(string p) => rules.Any(r => r.PlatformCode == p);
    var useMili = Allows("Mili") && mili.IsEnabled("Roofing");
    var useBlue = Allows("BLUEINK") && blue.IsEnabled("Roofing");
    // Wait for Mili's genuine one-per-minute limit before starting this lead's
    // auction. A durable ping reservation is visible to the ordinary workers.
    if (useMili) await ReserveMili(db, original, rules.Single(r => r.PlatformCode == "Mili"));
    var facts = AuctionFacts.From(lead, "Roofing");
    using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(90));
    var tasks = new List<Task<OfferResult>>();
    if (useBlue) tasks.Add(Receive("BLUEINK", () => blue.PingWithComplianceAsync(facts, lead.TrustedFormCertificateUrl, lead.JornayaLeadId, true, deadline.Token)));
    if (useMili) tasks.Add(Receive("Mili", () => mili.PingSharedAsync(facts, deadline.Token)));
    var offers = await Task.WhenAll(tasks);
    var protectedOffers = offers.Select(o => o with { Offer = o.Offer with {
        Reference = string.IsNullOrWhiteSpace(o.Offer.Reference) ? null : protector.Protect(o.Offer.Reference),
        PostContext = o.Offer.PostContext == null ? null : protector.Protect(o.Offer.PostContext) } }).ToArray();
    await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET OffersJson=@offers,UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id;", ("@id", id), ("@offers", JsonSerializer.Serialize(protectedOffers)));
    foreach (var o in offers) Console.WriteLine($"Lead {id} / {o.Platform}: {(o.Valid ? $"bid {o.Offer.Amount}" : o.Offer.Error ?? "invalid/no bid")}.");
    var winner = offers.Where(o => o.Valid).OrderByDescending(o => o.Offer.Amount).ThenBy(o => o.ReceivedOn).FirstOrDefault();
    if (winner == null) {
        await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='NoBid',UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id;", ("@id", id));
        Console.WriteLine($"Lead {id}: NoBid; no contact data posted."); return;
    }
    var rule = rules.Single(r => r.PlatformCode == winner.Platform);
    // Cooperate with the ordinary provider-rule row lock when reserving capacity.
    using (var tx = c.BeginTransaction(IsolationLevel.Serializable)) {
        await ScalarTx(c, "SELECT Id FROM VelonicDBUser.LeadRoutingRules WITH(UPDLOCK,HOLDLOCK) WHERE Id=@rule;", tx, ("@rule", rule.Id));
        var since = DateTime.UtcNow.Date;
        var count = Convert.ToInt32(await ScalarTx(c, "SELECT (SELECT COUNT(*) FROM VelonicDBUser.LeadRoutingAttempts a JOIN VelonicDBUser.LeadRoutingRuns r ON r.Id=a.LeadRoutingRunId WHERE a.LeadRoutingRuleId=@rule AND (a.IsWinner=1 OR a.Status='Delivered') AND COALESCE(r.WinnerLockedOn,a.CompletedOn)>=@since)+(SELECT COUNT(*) FROM VelonicDBUser.LeadSupplementalAuctions WHERE WinnerPlatformCode=@platform AND WinnerLockedOn>=@since);", tx, ("@rule", rule.Id), ("@since", since), ("@platform", winner.Platform)));
        if (rule.DailyCap is > 0 && count >= rule.DailyCap) {
            tx.Rollback(); await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='CapBlocked',UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id;", ("@id", id)); return;
        }
        var locked = await ExecTx(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='WinnerSelected',WinnerPlatformCode=@platform,WinningBidAmount=@amount,WinnerLockedOn=SYSUTCDATETIME(),UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id AND Status='Pinging' AND WinnerPlatformCode IS NULL AND PostAttempts=0;", tx, ("@id", id), ("@platform", winner.Platform), ("@amount", winner.Offer.Amount!));
        if (locked != 1) throw new InvalidOperationException("Winner already locked.");
        tx.Commit();
    }
    if (winner.Offer.ExpiresOn <= DateTime.UtcNow) {
        await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='Expired',ErrorCategory='Winning offer expired before post; no fallback.',UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id;", ("@id", id)); return;
    }
    var fence = await Exec(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status='Posting',PostAttempts=1,PostStartedOn=SYSUTCDATETIME(),UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id AND Status='WinnerSelected' AND PostAttempts=0;", ("@id", id));
    if (fence != 1) throw new InvalidOperationException("Post already reserved.");
    AuctionPost result;
    using var postTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
    try {
        result = winner.Platform == "BLUEINK"
            ? await blue.PostAsync(lead, "Roofing", winner.Offer.Reference!, winner.Offer.PostContext, $"HOMEYY-SUPPLEMENTAL-{id}", postTimeout.Token)
            : await mili.PostSharedAsync(lead, "Roofing", winner.Offer.Reference!, winner.Offer.PostContext, postTimeout.Token);
    } catch { result = new(false, false, true, null, "{}", "{}", "Post outcome unknown; reconcile with supplemental winner."); }
    var status = result.Accepted ? "Delivered" : result.OutcomeUnknown ? "ReconciliationRequired" : "PostRejected";
    using (var tx = c.BeginTransaction()) {
        await ExecTx(c, "UPDATE VelonicDBUser.LeadSupplementalAuctions SET Status=@status,PostCompletedOn=CASE WHEN @accepted=1 THEN SYSUTCDATETIME() ELSE NULL END,ExternalReferenceId=@ref,HttpStatusCode=@http,ErrorCategory=@error,UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id AND Status='Posting';", tx, ("@id", id), ("@status", status), ("@accepted", result.Accepted), ("@ref", result.DeliveryId), ("@http", result.HttpStatus), ("@error", result.Error));
        // Preserve historical failed/legacy records; only a confirmed new receipt
        // is written as an additional accepted delivery. Never touch NETWORX.
        if (result.Accepted) await ExecTx(c, "IF EXISTS(SELECT 1 FROM VelonicDBUser.ExternalLeadDeliveries WHERE LeadId=@id AND PlatformCode=@platform) UPDATE VelonicDBUser.ExternalLeadDeliveries SET Status='Delivered',DeliveredOn=SYSUTCDATETIME(),ExternalReferenceId=@ref,HttpStatusCode=@http,UpdatedOn=SYSUTCDATETIME() WHERE LeadId=@id AND PlatformCode=@platform AND DeliveredOn IS NULL; ELSE INSERT VelonicDBUser.ExternalLeadDeliveries(LeadId,PlatformCode,VerticalCode,Status,AttemptCount,MaxAttempts,CreatedOn,UpdatedOn,DeliveredOn,ExternalReferenceId,HttpStatusCode) VALUES(@id,@platform,'Roofing','Delivered',1,1,SYSUTCDATETIME(),SYSUTCDATETIME(),SYSUTCDATETIME(),@ref,@http);", tx, ("@id", id), ("@platform", winner.Platform), ("@ref", result.DeliveryId), ("@http", result.HttpStatus));
        tx.Commit();
    }
    Console.WriteLine($"Lead {id}: {status}; supplemental winner {winner.Platform}; bid {winner.Offer.Amount}. Payable earnings unverified.");
}

async Task ReserveMili(MyAppDbContext db, LeadRoutingRun original, LeadRoutingRule rule) {
    while (true) {
        TimeSpan wait;
        await using (var quota = await LeadExecutionLock.AcquireResourceAsync(db, "Homeyy.Mili.PingQuota.Roofing", default)) {
            if (quota == null) wait = TimeSpan.FromSeconds(5);
            else {
                var now = DateTime.UtcNow;
                var times = await db.LeadRoutingAttempts.Where(a => a.PlatformCode == "Mili" && a.LeadRoutingRun.VerticalCode == "Roofing" && a.PingDispatchedOn >= now.AddDays(-1))
                    .Select(a => a.PingDispatchedOn!.Value).ToListAsync();
                wait = MiliQuotaPolicy.Wait(times, now);
                if (wait <= TimeSpan.Zero) {
                    var last = await db.LeadRoutingAttempts.Where(a => a.LeadId == original.LeadId && a.PlatformCode == "Mili").Select(a => a.AttemptNumber).ToListAsync();
                    db.LeadRoutingAttempts.Add(new() { LeadId = original.LeadId, LeadRoutingRunId = original.Id, LeadRoutingRuleId = rule.Id,
                        PlatformCode = "Mili", DestinationType = "ExternalPlatform", Status = "SupplementalQuotaReserved", AttemptNumber = last.DefaultIfEmpty(0).Max()+1,
                        CreatedOn = now, PingDispatchedOn = now, ErrorMessage = "Ping-only quota reservation for approved supplemental batch unmatched-20261011; original winner unchanged." });
                    await db.SaveChangesAsync(); return;
                }
            }
        }
        if (wait > TimeSpan.FromMinutes(2)) throw new InvalidOperationException("Mili hourly/daily quota requires later review.");
        Console.WriteLine($"Lead {original.LeadId}: waiting {Math.Ceiling(wait.TotalSeconds)} seconds for Mili quota.");
        await Task.Delay(wait + TimeSpan.FromMilliseconds(100));
    }
}
async Task<OfferResult> Receive(string platform, Func<Task<AuctionOffer>> request) {
    var start = DateTime.UtcNow;
    AuctionOffer offer;
    try { offer = await request().WaitAsync(TimeSpan.FromSeconds(90)); }
    catch { offer = new(false, null, null, null, "{}", "{}", "Ping failed or timed out; no valid bid."); }
    var received = DateTime.UtcNow;
    var valid = AuctionPolicy.Reject(offer, received, start.AddSeconds(90), received) == null;
    return new(platform, offer, received, valid);
}
Task<object?> Scalar(SqlConnection c, string sql, params (string, object?)[] values) => ScalarTx(c, sql, null, values);
async Task<object?> ScalarTx(SqlConnection c, string sql, SqlTransaction? tx, params (string, object?)[] values) {
    await using var command = Command(c, sql, tx, values); return await command.ExecuteScalarAsync();
}
Task<int> Exec(SqlConnection c, string sql, params (string, object?)[] values) => ExecTx(c, sql, null, values);
async Task<int> ExecTx(SqlConnection c, string sql, SqlTransaction? tx, params (string, object?)[] values) {
    await using var command = Command(c, sql, tx, values); return await command.ExecuteNonQueryAsync();
}
SqlCommand Command(SqlConnection c, string sql, SqlTransaction? tx, (string Name, object? Value)[] values) {
    var command = c.CreateCommand(); command.Transaction = tx; command.CommandText = sql; command.CommandTimeout = 30;
    foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
    return command;
}
sealed record OfferResult(string Platform, AuctionOffer Offer, DateTime ReceivedOn, bool Valid);
