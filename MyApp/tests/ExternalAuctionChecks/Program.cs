using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyApp.Api.Controllers;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
using MyApp.Api.Services.Thumbtack;
using MyApp.Api.Services.ExternalDeliveries.BlueInk;
using MyApp.Api.Services.ExternalDeliveries.InsuranceTales;
using MyApp.Api.Services.ExternalDeliveries.Mili;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Services.ExternalDeliveries.Networx;

var passed = 0;
void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
async Task Check(string name, Func<Task> test) { await test(); Console.WriteLine("PASS " + name); passed++; }
Fixture New(params (string Code, decimal Bid, int Delay)[] offers) => new(offers);
await Check("service + ZIP match selects Thumbtack and prevents all pings/posts", async () => {
    var f = New(("A", 10, 0)); f.TT.Covered = true;
    await f.Run(); Assert(f.Store.Work.Run.Status == "ThumbtackSelected" && f.Gateway.Pings.Count == 0 && f.Gateway.Posts.Count == 0, "Thumbtack exclusivity");
    Assert(f.Store.Work.Lead.LeadStatus != "Sold", "Recommendations must not be sold");
});
await Check("uncovered ZIP opens auction", async () => { var f = New(("A", 10, 0)); await f.Run(); Assert(f.Store.Work.Run.Status == "Won", "Expected auction"); });
await Check("covered zero professionals never opens auction", async () => {
    var f = New(("A", 10, 0)); f.TT.Covered = true; f.TT.Available = false; await f.Run(); Assert(f.Store.Work.Run.Status == "ThumbtackSelected" && f.Gateway.Pings.Count == 0 && f.Gateway.Posts.Count == 0, "Covered no-results must stay exclusive");
});
await Check("Thumbtack API errors do not silently sell elsewhere", async () => {
    var f = New(("A", 10, 0)); f.TT.Covered = true; f.TT.Throw = true; await f.Run(); Assert(f.Store.Work.Run.Status == "ThumbtackFailed" && f.Gateway.Pings.Count == 0, "Unknown availability must fail closed");
});
await Check("highest bid wins, all eligible pings overlap", async () => {
    var f = New(("A", 9, 60), ("B", 20, 40), ("C", 12, 50)); await f.Run();
    Assert(f.Store.Work.Run.WinnerPlatformCode == "B" && f.Gateway.MaxConcurrent == 3, "Auction order or concurrency");
    Assert(f.Gateway.Posts.SequenceEqual(["B"]), "Only winner may post");
});
await Check("equal bids choose earliest received response", async () => { var f = New(("A", 10, 60), ("B", 10, 5)); await f.Run(); Assert(f.Store.Work.Run.WinnerPlatformCode == "B", "Tie order"); });
await Check("all final responses close early", async () => { var f = New(("A", 10, 5)); var sw = Stopwatch.StartNew(); await f.Run(); Assert(sw.Elapsed < TimeSpan.FromSeconds(2), "Waited full auction"); });
await Check("deadline bounds uncooperative ping; late bid cannot win", async () => {
    var f = New(("A", 10, 5), ("B", 999, 1400)); f.Options.AuctionWindowSeconds = 1; f.Gateway.IgnoreCancellation = true;
    await f.Run(); Assert(f.Store.Work.Run.WinnerPlatformCode == "A", "Late bidder won");
    Assert(f.Store.Work.Attempts.Single(x => x.PlatformCode == "B").Status == "TimedOut", "Timeout missing");
});
await Check("expired offer rejected", async () => { var f = New(("A", 10, 0)); f.Gateway.Expire = true; await f.Run(); Assert(f.Store.Work.Run.Status == "NoBid", "Expired bid accepted"); });
await Check("rejected offer cannot win", async () => { var f = New(("A", 10, 0)); f.Gateway.Reject = true; await f.Run(); Assert(f.Gateway.Posts.Count == 0, "Rejected bidder posted"); });
await Check("missing token cannot win", async () => { var f = New(("A", 10, 0)); f.Gateway.NoToken = true; await f.Run(); Assert(f.Store.Work.Run.Status == "NoBid", "Missing token accepted"); });
await Check("zero and invalid bids cannot win", () => {
    var now = DateTime.UtcNow; Assert(AuctionPolicy.Reject(new(true, 0, "token", null, "{}", "{}"), now, now, now) == "InvalidBid", "Zero bid");
    Assert(AuctionPolicy.Price("not-money") == null, "Invalid amount"); return Task.CompletedTask;
});
await Check("contact data cannot cross ping facts boundary", async () => {
    var f = New(("A", 10, 0), ("B", 5, 0)); await f.Run();
    var text = System.Text.Json.JsonSerializer.Serialize(f.Gateway.Facts);
    foreach (var secret in new[] { "Private Person", "private@example.test", "5551234567", "12 Private Street", "CERTIFICATE" }) Assert(!text.Contains(secret), "PII leaked into ping");
    Assert(f.Gateway.LastPostedLead?.Email == "private@example.test" && f.Gateway.Posts.Count == 1, "Winner missing contact or losers posted");
});
await Check("concurrent workers lock one winner and create one delivery", async () => {
    var f = New(("A", 10, 40)); await Task.WhenAll(f.Run(), f.Run(), f.Run());
    Assert(f.Store.WinnerCommits == 1 && f.Store.Deliveries == 1 && f.Gateway.Posts.Count == 1, "Duplicate winner/post");
});
await Check("repeated worker execution is inert after success", async () => { var f = New(("A", 10, 0)); await f.Run(); await f.Run(); Assert(f.Store.Deliveries == 1 && f.Gateway.Pings.Count == 1, "Repeated sale"); });
await Check("restart during collection preserves bids without repinging", async () => {
    var f = New(("A", 10, 0), ("B", 99, 0)); f.SeedBids(); f.Store.Work.Attempts[1].Status = "PingStarted";
    await f.Run(); Assert(f.Gateway.Pings.Count == 0 && f.Store.Work.Run.WinnerPlatformCode == "A", "Restart reping");
});
await Check("restart after winner lock resumes that winner only", async () => {
    var f = New(("A", 10, 0), ("B", 99, 0)); f.SeedBids(); await f.Store.LockWinnerAsync(f.Store.Work, f.Store.Work.Attempts[0], default);
    await f.Run(); Assert(f.Gateway.Posts.SequenceEqual(["A"]) && f.Gateway.Pings.Count == 0, "Restart changed winner");
});
await Check("crash during non-idempotent post requires reconciliation, never replay", async () => {
    var f = New(("A", 10, 0)); f.SeedBids(); await f.Store.LockWinnerAsync(f.Store.Work, f.Store.Work.Attempts[0], default); f.Store.Work.Run.Status = "Posting";
    await f.Run(); Assert(f.Gateway.Posts.Count == 0 && f.Store.Work.Run.Status == "ReconciliationRequired", "Unsafe post replay");
});
await Check("definitive retry-safe failure retries only locked winner", async () => {
    var f = New(("A", 10, 0), ("B", 5, 0)); f.Gateway.SafeFailures = 1; await f.Run(); f.Store.Work.Run.NextAttemptOn = DateTime.UtcNow.AddSeconds(-1); await f.Run();
    Assert(f.Gateway.Posts.SequenceEqual(["A", "A"]) && f.Store.Work.Run.Status == "Won", "Winner retry");
});
await Check("ambiguous non-idempotent post is not retried", async () => {
    var f = New(("A", 10, 0), ("B", 5, 0)); f.Gateway.UnknownPost = true; await f.Run(); await f.Run();
    Assert(f.Gateway.Posts.SequenceEqual(["A"]) && f.Store.Work.Run.Status == "ReconciliationRequired", "Ambiguous post fallback");
});
await Check("winner fallback disabled by default", () => { Assert(!new ExternalLeadAuctionOptions().AllowWinnerFallback, "Fallback default"); return Task.CompletedTask; });
await Check("disabled provider excluded before ping", async () => { var f = New(("A", 10, 0)); f.Gateway.Disabled = true; await f.Run(); Assert(f.Gateway.Pings.Count == 0 && f.Store.Work.Attempts[0].Status == "Excluded", "Disabled provider pinged"); });
await Check("cap race rejects bid before locking and posting", async () => { var f = New(("A", 20, 0), ("B", 5, 0)); f.Store.CapReject = "A"; await f.Run(); Assert(f.Gateway.Posts.SequenceEqual(["B"]), "Cap race"); });
await Check("leading zeros and unmapped coverage preview", () => {
    Assert(ThumbtackCoverageLookup.NormalizeZip("1007") == "01007" && ThumbtackCoverageLookup.NormalizeZip("01007") == "01007", "ZIP padding");
    var p = CoverageParser.Parse("Service,Zip\nRoofing,1007\nUnknown,99208", null, ["Roofing"]);
    Assert(p.Rows.Single().Zip == "01007" && p.Errors.Single().Line == 3, "Unmapped row disappeared"); return Task.CompletedTask;
});
await Check("coverage parser retains duplicate input for idempotent upsert", () => {
    var p = CoverageParser.Parse("Zip Code\n1007\n01007", "Roofing", ["Roofing"]);
    Assert(p.Errors.Count == 0 && p.Rows.DistinctBy(x => (x.Service, x.Zip)).Count() == 1, "Coverage duplicate normalization"); return Task.CompletedTask;
});
await Check("BlueInk split adapter ping omits contacts; post uses stored token", async () => {
    var handler = new CaptureHandler(["{\"status\":\"success\",\"price\":\"25\",\"auth_code\":\"offer-token\"}"]);
    var provider = new BlueInkLeadProvider(new HttpClient(handler), Options.Create(new BlueInkOptions { Enabled = true,
        Verticals = new(StringComparer.OrdinalIgnoreCase) { ["Roofing"] = new() { Enabled = true, Token = "TEST-SECRET" } } }), NullLogger<BlueInkLeadProvider>.Instance);
    var lead = Fixture.Lead(); var offer = await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
    Assert(offer.Amount == 25 && offer.Reference == "offer-token", "Bid parser");
    foreach (var value in new[] { lead.Email!, lead.Phone!, lead.Address!, lead.FullName!, lead.TrustedFormCertificateUrl! }) Assert(!handler.Bodies.Single().Contains(value), "Adapter ping PII");
    Assert(!offer.RequestAudit.Contains("TEST-SECRET") && !offer.ResponseAudit.Contains("offer-token"), "Audit leaked token");
    var post = await provider.PostAsync(lead, "Roofing", offer.Reference!, null, "test-key", default);
    Assert(post.Accepted && handler.Bodies.Count == 2 && handler.Bodies[1].Contains("offer-token") && handler.Bodies[1].Contains(lead.Email!), "Separate post did not use token/contact");
    Assert(!post.RequestAudit.Contains(lead.Email!) && !post.RequestAudit.Contains(lead.FullName!), "Post audit leaked PII");
});
await Check("InsuranceTales XML bid/token and contact-free ping", async () => {
    var handler = new CaptureHandler(["<response><result>success</result><ping_id>xml-token</ping_id><price>18.75</price></response>"]);
    var provider = new InsuranceTalesLeadProvider(new HttpClient(handler), Options.Create(new InsuranceTalesOptions {
        Enabled = true, Windows = new() { Enabled = true, CampaignId = "test", CampaignKey = "TEST-SECRET" } }), NullLogger<InsuranceTalesLeadProvider>.Instance);
    var lead = Fixture.Lead(); lead.LeadTypeId = 2;
    var bid = await provider.PingAsync(AuctionFacts.From(lead, "Windows"), default);
    Assert(bid.Accepted && bid.Amount == 18.75m && bid.Reference == "xml-token", "InsuranceTales response mapping");
    foreach (var secret in new[] { lead.Email!, lead.Phone!, lead.FullName!, lead.Address!, lead.TrustedFormCertificateUrl! })
        Assert(!handler.Bodies.Single().Contains(secret), "InsuranceTales ping contact leak");
});
await Check("audit redacts credentials, tokens, free text and contacts recursively", () => {
    var audit = AuctionAudit.Payload(new { auth_code = "SECRET", data = new { project_type = "Private Person", zip_code = "01007" }, contact = new { email = "private@example.test" } });
    Assert(!audit.Contains("SECRET") && !audit.Contains("Private Person") && !audit.Contains("private@example.test") && audit.Contains("01007"), "Audit redaction");
    return Task.CompletedTask;
});
await Check("Mili uses separate contact-free ping and stored-token post", async () => {
    var handler = new CaptureHandler(["<response><result>success</result><ping_id>mili-token</ping_id><price>30</price></response>",
        "<response><result>success</result><lead_id>mili-delivery</lead_id></response>"]);
    var provider = new MiliLeadProvider(new HttpClient(handler), Options.Create(new MiliOptions { Enabled = true,
        Roofing = new() { Enabled = true, CampaignId = "test", CampaignKey = "TEST-SECRET" } }), new MiliRateLimiter(),
        NullLogger<MiliLeadProvider>.Instance, new NoonPacific());
    var lead = Fixture.Lead(); var offer = await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
    Assert(offer.Accepted && offer.Amount == 30 && offer.Reference == "mili-token", "Mili bid parsing");
    foreach (var secret in new[] { lead.Email!, lead.Phone!, lead.FullName!, lead.Address!, lead.TrustedFormCertificateUrl! })
        Assert(!Uri.UnescapeDataString(handler.Bodies[0]).Contains(secret), "Mili ping contact leak");
    var result = await provider.PostAsync(lead, "Roofing", offer.Reference!, offer.PostContext, "stable-key", default);
    Assert(result.Accepted && handler.Bodies.Count == 2 && handler.Bodies[1].Contains("mili-token"), "Mili separate post");
});
await Check("Networx existing XML contract parses price and post token", () => {
    var method = typeof(MyApp.Api.Services.ExternalDeliveries.Networx.NetworxLeadProvider)
        .GetMethod("ParseResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
    var result = method.Invoke(null, ["<response><statusCode>200</statusCode><token>net-token</token><price>42.50</price></response>"])!;
    Assert((string?)result.GetType().GetProperty("Token")!.GetValue(result) == "net-token", "Networx token");
    Assert(AuctionPolicy.Price((string?)result.GetType().GetProperty("Price")!.GetValue(result)) == 42.5m, "Networx price");
    return Task.CompletedTask;
});
await Check("legacy FallThrough cannot override covered ownership", async () => {
    var f = New(("A", 10, 0)); f.TT.Covered = true; f.TT.Throw = true; f.Options.ThumbtackErrorPolicy = "FallThrough";
    await f.Run(); Assert(f.Gateway.Pings.Count == 0 && f.Gateway.Posts.Count == 0 && f.Store.Work.Run.Status == "ThumbtackFailed", "Coverage ownership overridden");
});
await Check("persisted Thumbtack search intent is never repeated after restart", async () => {
    var f = New(("A", 10, 0)); f.Store.Work.Run.Status = "CheckingThumbtack";
    f.Store.Work.Run.ThumbtackSearchStartedOn = DateTime.UtcNow; f.TT.Throw = true;
    await f.Run(); Assert(f.TT.Calls == 0 && f.Gateway.Pings.Count == 0 && f.Store.Work.Run.Status == "ThumbtackFailed", "Search replay");
});
await Check("shutdown cancellation preserves ping intents without posting", async () => {
    var f = New(("A", 10, 200)); using var cts = new CancellationTokenSource(30);
    try { await new ExternalAuctionEngine(f.Store, f.Gateway, f.TT, Options.Create(f.Options)).ProcessAsync(1, cts.Token); throw new Exception("Cancellation swallowed"); }
    catch (OperationCanceledException) { }
    Assert(f.Gateway.Posts.Count == 0 && f.Store.Work.Run.Status == "CollectingBids", "Cancelled run posted");
    await f.Run(); Assert(f.Gateway.Pings.Count == 1 && f.Gateway.Posts.Count == 0, "Cancellation recovery replayed ping");
});
await Check("admin reconciliation records accepted outcome once without posting", async () => {
    var f = New(("A", 10, 0)); f.Gateway.UnknownPost = true; await f.Run();
    var engine = new ExternalAuctionEngine(f.Store, f.Gateway, f.TT, Options.Create(f.Options));
    Assert(await engine.ReconcileAsync(1, true, "CASE-123", "admin-id", default), "Reconcile refused");
    Assert(!await engine.ReconcileAsync(1, true, "CASE-123", "admin-id", default), "Duplicate reconciliation");
    Assert(f.Store.Work.Run.Status == "Won" && f.Store.Work.Run.ReconciledBy == "admin-id" && f.Gateway.Posts.Count == 1, "Reconciliation sent data");
});
await Check("confirmed nonacceptance closes without fallback or retry", async () => {
    var f = New(("A", 10, 0), ("B", 5, 0)); f.Gateway.UnknownPost = true; await f.Run();
    await new ExternalAuctionEngine(f.Store, f.Gateway, f.TT, Options.Create(f.Options)).ReconcileAsync(1, false, "CASE-321", "admin-id", default);
    await f.Run(); Assert(f.Gateway.Posts.SequenceEqual(["A"]) && f.Store.Work.Run.Status == "Failed", "Reconciliation fallback");
});
await Check("nested form qualifications retain numeric strings without leaking contacts", () => {
    var lead = Fixture.Lead(); lead.AdditionalDataJson = "{\"answers\":{\"projectType\":\"Replace\",\"numWindows\":\"3\",\"phone\":\"5551234567\"}}";
    var facts = AuctionFacts.From(lead, "Windows");
    Assert(NetworxLeadProvider.AuctionTaskOption(facts) == "WINDOW_INSTALL_MULTIPLE" && !facts.Qualifications.Contains("5551234567"), "Nested mapping");
    return Task.CompletedTask;
});
await Check("Networx unsupported task rejects before database or HTTP", async () => {
    using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
    var handler = new CaptureHandler(["unused"]);
    var provider = new NetworxLeadProvider(new HttpClient(handler), db, Options.Create(new NetworxOptions()), NullLogger<NetworxLeadProvider>.Instance);
    var offer = await provider.PingAsync(AuctionFacts.From(Fixture.Lead(), "Gutters"), default);
    Assert(!offer.Accepted && handler.Bodies.Count == 0 && !provider.IsEnabled("Gutters"), "Unsupported mapping sent HTTP");
});
await Check("Networx missing or ambiguous configured task blocks HTTP with exact option", async () => {
    using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
    var lead = Fixture.Lead(); lead.AdditionalDataJson = "{\"projectType\":\"Replace\",\"roofingType\":\"Asphalt\"}";
    foreach (var tasks in new List<string>[] { [], ["one", "two"], [""] }) {
        var handler = new CaptureHandler(["unused"]);
        var p = new NetworxLeadProvider(new HttpClient(handler), db, Options.Create(new NetworxOptions()), NullLogger<NetworxLeadProvider>.Instance, new TestTaskLookup(tasks));
        var offer = await p.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
        Assert(!offer.Accepted && handler.Bodies.Count == 0 && offer.Error!.Contains("ROOF_REPLACE_ASPHALT"), "Missing/ambiguous task sent HTTP");
    }
});
await Check("Networx actual split adapter sends mapped task, token and winner-only contact", async () => {
    using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
    var handler = new CaptureHandler(["<response><statusCode>200</statusCode><token>net-token</token><price>42.50</price></response>"]);
    var p = new NetworxLeadProvider(new HttpClient(handler), db, Options.Create(new NetworxOptions { Enabled = true, UserId = "test-user", AccessKey = "TEST-SECRET" }), NullLogger<NetworxLeadProvider>.Instance, new TestTaskLookup(["mapped-task"]));
    var lead = Fixture.Lead(); lead.AdditionalDataJson = "{\"projectType\":\"Replace\",\"roofingType\":\"Asphalt\"}";
    var offer = await p.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
    Assert(offer.Accepted && offer.Amount == 42.5m && offer.PostContext == "mapped-task", "Task/bid not retained");
    var ping = Uri.UnescapeDataString(handler.Bodies.Single());
    foreach (var secret in new[] { lead.Email!, lead.Phone!, lead.FullName!, lead.Address!, lead.TrustedFormCertificateUrl! }) Assert(!ping.Contains(secret), "Networx ping contact leak");
    var post = await p.PostAsync(lead, "Roofing", offer.Reference!, offer.PostContext, "stable-key", default);
    Assert(post.Accepted && handler.Bodies.Count == 2 && handler.Bodies[1].Contains("net-token"), "Networx separate post");
});
await Check("Networx readiness enumerates all 15 existing option mappings", () => {
    Assert(NetworxLeadProvider.RequiredTaskOptions.Values.Sum(x => x.Length) == 15, "Mapping manifest"); return Task.CompletedTask;
});
await Check("Modernize cannot enter monetary auction adapter registry", () => {
    Assert(!typeof(IAuctionProvider).IsAssignableFrom(typeof(MyApp.Api.Services.ExternalDeliveries.Modernize.ModernizeLeadProvider)) &&
        !AuctionGateway.Capabilities.Single(x => x.PlatformCode == "Modernize").MonetaryBid, "Modernize entered auction"); return Task.CompletedTask;
});
await Check("duplicate lead queue policy refuses prior run or any prior provider delivery", () => {
    Assert(RoutingActivation.CanCreateAuction(false, false) && !RoutingActivation.CanCreateAuction(true, false) &&
        !RoutingActivation.CanCreateAuction(false, true) && !RoutingActivation.CanCreateAuction(true, true), "Duplicate queue policy"); return Task.CompletedTask;
});
await Check("feature off preserves both existing routing activation paths", () => {
    Assert(RoutingActivation.Mode(false, false) == "LegacyDistribution" && RoutingActivation.Mode(false, true) == "LegacyExclusive" &&
        RoutingActivation.Mode(true, false) == "Auction", "Legacy activation changed"); return Task.CompletedTask;
});
await Check("all four Thumbtack services resolve including shared lead-type Gutters", () => {
    var resolve = typeof(MyApp.Api.Services.ExternalDeliveries.ExternalLeadDistributionService)
        .GetMethod("ResolveVertical", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
    foreach (var service in new[] { "Roofing", "Windows", "Bathroom", "Gutters" }) {
        var lead = Fixture.Lead(); lead.LeadTypeId = 2; lead.AdditionalDataJson = "{\"serviceCode\":\"" + service + "\"}";
        Assert((string?)resolve.Invoke(null, [lead, true]) == service, "Service normalization");
        if (service == "Gutters") Assert(resolve.Invoke(null, [lead, false]) == null, "Legacy eligibility changed");
    }
    return Task.CompletedTask;
});
await Check("unsafe auction startup configuration rejected", () => {
    var o = new ExternalLeadAuctionOptions(); Assert(ExternalLeadAuctionOptions.Valid(o), "Safe defaults rejected");
    o.Enabled = true; Assert(!ExternalLeadAuctionOptions.Valid(o), "Missing durable key ring");
    o.KeyRingPath = Path.GetFullPath("artifacts/test-key-ring"); Assert(ExternalLeadAuctionOptions.Valid(o), "Valid settings rejected");
    o.AllowWinnerFallback = true; Assert(!ExternalLeadAuctionOptions.Valid(o), "Fallback enabled");
    o.AllowWinnerFallback = false; o.ThumbtackErrorPolicy = "Ignore"; Assert(!ExternalLeadAuctionOptions.Valid(o), "Unknown TT policy accepted");
    o.ThumbtackErrorPolicy = "FailClosed"; o.AuctionWindowSeconds = 0; Assert(!ExternalLeadAuctionOptions.Valid(o), "Invalid timing"); return Task.CompletedTask;
});
await Check("SQL monetary precision rejects rounding and overflow", () => {
    var now = DateTime.UtcNow;
    foreach (var value in new[] { 1.12345m, 100000000000000m }) Assert(AuctionPolicy.Reject(new(true, value, "t", null, "{}", "{}"), now, now, now) == "InvalidBid", "Unsafe SQL amount");
    Assert(AuctionPolicy.Price("1,2") == null, "Malformed amount accepted"); return Task.CompletedTask;
});
await Check("admin coverage and reconciliation endpoints require admin authorization", () => {
    foreach (var type in new[] { typeof(LeadRoutingController), typeof(ThumbtackCoverageController) })
        Assert(type.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any(x => x.Roles == "SuperAdmin,Admin"), "Missing authorization");
    return Task.CompletedTask;
});
await Check("all 75 routing entity properties match deployment SQL types and nullability", () => {
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
    var sql = File.ReadAllText(Path.Combine(root, "database/external-lead-auction.sql"));
    var columns = System.Text.RegularExpressions.Regex.Matches(sql, @"\('(LeadRouting\w+)','(\w+)','((?:''|[^'])*)'\)")
        .ToDictionary(m => (m.Groups[1].Value, m.Groups[2].Value), m => m.Groups[3].Value);
    using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
    var count = 0;
    foreach (var type in new[] { typeof(LeadRoutingRun), typeof(LeadRoutingAttempt), typeof(LeadRoutingRule) }) {
        var entity = db.Model.FindEntityType(type)!;
        foreach (var p in entity.GetProperties()) {
            var definition = columns[(entity.GetTableName()!, p.GetColumnName())];
            Assert(definition.Split(' ')[0].Replace(" ", "") == p.GetColumnType()!.Replace(" ", ""), $"SQL type mismatch {type.Name}.{p.Name}");
            Assert(definition.Contains("NOT NULL") != p.IsNullable, $"SQL nullability mismatch {type.Name}.{p.Name}"); count++;
        }
    }
    Assert(count == columns.Count, "SQL/entity column count mismatch"); return Task.CompletedTask;
});
await Check("covered timeout retains Thumbtack ownership with every external platform suppressed", async () => {
    var f = New(("BLUEINK", 10, 0), ("NETWORX", 11, 0), ("Mili", 12, 0), ("InsuranceTales", 13, 0), ("Modernize", 14, 0));
    f.TT.Covered = true; f.TT.Timeout = true;
    await f.Run(); await f.Run();
    Assert(f.Store.Work.Run.Status == "ThumbtackFailed" && f.Store.Work.Run.WinnerPlatformCode == "THUMBTACK" &&
        f.Store.Work.Run.ErrorMessage != null && f.Gateway.Pings.Count == 0 && f.Gateway.Posts.Count == 0 && f.TT.Calls == 1, "Timeout crossed routes");
});
await Check("unsupported Thumbtack service goes directly to auction without lookup", async () => {
    var f = New(("A", 10, 0)); f.Store.Work.Run.VerticalCode = "HVAC"; await f.Run();
    Assert(f.TT.Calls == 0 && f.Gateway.Posts.Count == 1, "Unsupported service searched Thumbtack");
});
await Check("uncovered pair never calls Thumbtack even if lookup would fail", async () => {
    var f = New(("A", 10, 0)); f.TT.Throw = true; await f.Run();
    Assert(f.TT.Calls == 0 && f.Gateway.Pings.Count == 1, "Uncovered lookup");
});
await Check("coverage read failure cannot authorize either route even with legacy FallThrough", async () => {
    var f = New(("A", 10, 0)); f.TT.CoverageError = true; f.Options.ThumbtackErrorPolicy = "FallThrough"; await f.Run();
    Assert(f.Store.Work.Run.Status == "Failed" && f.TT.Calls == 0 && f.Gateway.Pings.Count == 0, "Unknown coverage leaked");
});
await Check("all covered outcomes remain exclusive across concurrent and repeated processing", async () => {
    foreach (var outcome in new[] { "results", "empty", "error", "timeout" }) {
        var f = New(("A", 10, 0)); f.TT.Covered = true; f.TT.Available = outcome != "empty";
        f.TT.Throw = outcome == "error"; f.TT.Timeout = outcome == "timeout";
        await Task.WhenAll(f.Run(), f.Run(), f.Run()); await f.Run();
        Assert(f.TT.Calls == 1 && f.Gateway.Pings.Count == 0 && f.Gateway.Posts.Count == 0 && f.Store.Deliveries == 0, outcome + " lost exclusivity");
    }
});
await Check("persisted Thumbtack ownership survives coverage removal before lookup", async () => {
    var f = New(("A", 10, 0)); f.Store.Work.Run.Status = "CheckingThumbtack";
    f.Store.Work.Run.WinnerPlatformCode = "THUMBTACK"; f.TT.Covered = false; await f.Run();
    Assert(f.Gateway.Pings.Count == 0 && f.Store.Work.Run.Status == "ThumbtackFailed", "Coverage change transferred ownership");
});
Console.WriteLine($"All {passed} offline checks passed. No database or live-provider connections.");

sealed class Fixture
{
    public MemoryStore Store; public FakeGateway Gateway; public FakeThumbtack TT = new(); public ExternalLeadAuctionOptions Options = new();
    public Fixture((string Code, decimal Bid, int Delay)[] offers)
    {
        Store = new(new(new LeadRoutingRun { Id = 1, LeadId = 1, VerticalCode = "Roofing", RoutingMode = "Auction", Status = "Pending" }, Lead(), []));
        Store.Rules = offers.Select((x, i) => new LeadRoutingRule { Id = i + 1, PlatformCode = x.Code }).ToList();
        Gateway = new(offers);
    }
    public Task Run() => new ExternalAuctionEngine(Store, Gateway, TT, Microsoft.Extensions.Options.Options.Create(Options)).ProcessAsync(1, default);
    public void SeedBids()
    {
        Store.Work.Run.Status = "CollectingBids"; Store.Work.Run.AuctionClosesOn = DateTime.UtcNow.AddSeconds(90);
        foreach (var x in Gateway.Offers) Store.Work.Attempts.Add(new() { Id = Store.Work.Attempts.Count + 1, LeadRoutingRunId = 1,
            PlatformCode = x.Code, Status = "BidAccepted", PingReferenceId = "encrypted-token", OfferedBidAmount = x.Bid, BidReceivedOn = DateTime.UtcNow });
    }
    public static Lead Lead() => new() { Id = 1, LeadTypeId = 1, IsCompleted = true, Postcode = "01007", OwnsProperty = true,
        FullName = "Private Person", Email = "private@example.test", Phone = "5551234567", Address = "12 Private Street",
        TrustedFormCertificateUrl = "CERTIFICATE", LeadStatus = "New", IpAddress = "192.0.2.1", City = "Spokane", State = "WA",
        LandingPageUrl = "https://homeyy.com/roofing?email=private@example.test", TcpaComplianceText = "Test consent",
        AdditionalDataJson = "{\"email\":\"private@example.test\",\"projectType\":\"Roof Replacement\",\"phone\":\"5551234567\"}" };
}
sealed class MemoryStore(AuctionWork work) : IAuctionStore
{
    public AuctionWork Work = work; public List<LeadRoutingRule> Rules = []; public int WinnerCommits, Deliveries; public string? CapReject;
    private readonly SemaphoreSlim gate = new(1);
    public Task<AuctionWork?> LoadAsync(long id, CancellationToken ct) => Task.FromResult<AuctionWork?>(Work);
    public async Task<IAsyncDisposable?> LockAsync(long id, CancellationToken ct) => await gate.WaitAsync(0, ct) ? new Unlock(gate) : null;
    public Task<List<LeadRoutingRule>> RulesAsync(AuctionWork w, CancellationToken ct) => Task.FromResult(Rules);
    public Task SaveAsync(AuctionWork w, CancellationToken ct) { foreach (var a in w.Attempts.Where(x => x.Id == 0)) a.Id = w.Attempts.IndexOf(a) + 1; return Task.CompletedTask; }
    public Task<bool> LockWinnerAsync(AuctionWork w, LeadRoutingAttempt a, CancellationToken ct)
    {
        lock (Work)
        {
            if (a.PlatformCode == CapReject) return Task.FromResult(false);
            if (w.Run.WinnerAttemptId != null) return Task.FromResult(w.Run.WinnerAttemptId == a.Id);
            w.Run.WinnerAttemptId = a.Id; w.Run.WinnerPlatformCode = a.PlatformCode; w.Run.Status = "WinnerSelected"; a.IsWinner = true; WinnerCommits++;
            return Task.FromResult(true);
        }
    }
    public Task RecordDeliveryAsync(AuctionWork w, LeadRoutingAttempt a, AuctionPost result, CancellationToken ct) { if (result.Accepted) Deliveries++; return Task.CompletedTask; }
    sealed class Unlock(SemaphoreSlim gate) : IAsyncDisposable { public ValueTask DisposeAsync() { gate.Release(); return ValueTask.CompletedTask; } }
}
sealed class FakeThumbtack : IThumbtackAuctionGateway
{
    public bool Covered, Throw, Timeout, CoverageError; public bool Available = true; public int Calls;
    public Task<bool> IsCoveredAsync(AuctionWork w, CancellationToken ct) => CoverageError ? throw new Exception("coverage unavailable") : Task.FromResult(Covered);
    public Task<ThumbtackCheck> CheckAsync(AuctionWork w, CancellationToken ct) { Calls++; if (Timeout) throw new TaskCanceledException("lookup timeout"); return Throw ? throw new Exception("test") :
        Task.FromResult(new ThumbtackCheck(Covered, Covered && Available, "test coverage", "search-id", "{}")); }
}
sealed class FakeGateway((string Code, decimal Bid, int Delay)[] offers) : IAuctionGateway
{
    public (string Code, decimal Bid, int Delay)[] Offers = offers;
    public ConcurrentBag<string> Pings = []; public List<string> Posts = []; public ConcurrentBag<AuctionFacts> Facts = [];
    public bool IgnoreCancellation, Expire, Reject, NoToken, Disabled, UnknownPost; public int SafeFailures, MaxConcurrent; private int active;
    public Lead? LastPostedLead;
    public bool Enabled(string p, string v) => !Disabled;
    public bool Idempotent(string p) => false;
    public string Protect(string value) => "encrypted-" + value;
    public string Unprotect(string value) => value.Replace("encrypted-", "");
    public async Task<AuctionOffer> PingAsync(string p, AuctionFacts facts, CancellationToken ct)
    {
        Pings.Add(p); Facts.Add(facts); var n = Interlocked.Increment(ref active); MaxConcurrent = Math.Max(MaxConcurrent, n);
        var offer = Offers.Single(x => x.Code == p);
        try { await Task.Delay(offer.Delay, IgnoreCancellation ? default : ct); }
        finally { Interlocked.Decrement(ref active); }
        return new(!Reject, offer.Bid, NoToken ? null : "token", Expire ? DateTime.UtcNow.AddSeconds(-1) : null, "{}", "{}");
    }
    public Task<AuctionPost> PostAsync(string p, AuctionWork w, string reference, string? context, string key, CancellationToken ct)
    {
        Posts.Add(p); LastPostedLead = w.Lead;
        if (UnknownPost) return Task.FromResult(new AuctionPost(false, true, true, null, "{}", "{}"));
        if (SafeFailures-- > 0) return Task.FromResult(new AuctionPost(false, true, false, null, "{}", "{}"));
        return Task.FromResult(new AuctionPost(true, false, false, "delivery-id", "{}", "{}"));
    }
}
sealed class CaptureHandler(string[] responses) : HttpMessageHandler
{
    public List<string> Bodies = []; private int index;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    { Bodies.Add(await request.Content!.ReadAsStringAsync(ct)); return new(HttpStatusCode.OK) { Content = new StringContent(responses[Math.Min(index++, responses.Length - 1)], Encoding.UTF8) }; }
}
sealed class NoonPacific : TimeProvider { public override DateTimeOffset GetUtcNow() => new(2026, 9, 28, 19, 0, 0, TimeSpan.Zero); }
sealed class TestTaskLookup(List<string> tasks) : INetworxAuctionTaskLookup
{ public Task<List<string>> FindAsync(int leadTypeId, string option, CancellationToken ct) => Task.FromResult(tasks); }
