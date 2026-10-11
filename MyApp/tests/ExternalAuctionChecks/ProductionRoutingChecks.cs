using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;
using MyApp.Api.Services.ExternalDeliveries;
using MyApp.Api.Services.ExternalDeliveries.BlueInk;
using MyApp.Api.Services.Routing;
using MyApp.Api.Services.Thumbtack;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Controllers;
using MyApp.Api.DTOs.Leads;
using MyApp.Api.DTOs.Location;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.Fraud;
using MyApp.Api.Services.Location;
using MyApp.Api.Services.MetaLeads;

static class ProductionRoutingChecks
{
    public static async Task Run(Func<string, Func<Task>, Task> check, Action<bool, string> assert)
    {
        await check("SQL auction store requires an approval for this lead and exact run, after creation", async () => {
            await using var f = await ProductionFixture.Create();
            await f.Router.CompleteAsync(f.Lead);
            var run = await f.Db.LeadRoutingRuns.SingleAsync();
            var gateway = new Fixture([]).Gateway;
            var store = new SqlAuctionStore(f.Db, gateway);
            assert(!(await store.LoadAsync(run.Id, default))!.RiskReviewApproved, "Unapproved run bypassed risk");
            var history = new LeadStatusHistory { LeadId = f.Lead.Id, NewStatus = "New", ChangedOn = run.CreatedOn.AddSeconds(1), Remarks = LeadRoutingRiskPolicy.ApprovalRemark(run.Id + 1) };
            f.Db.LeadStatusHistories.Add(history); await f.Db.SaveChangesAsync();
            assert(!(await store.LoadAsync(run.Id, default))!.RiskReviewApproved, "Approval applied to another run");
            history = await f.Db.LeadStatusHistories.SingleAsync(); history.Remarks = LeadRoutingRiskPolicy.ApprovalRemark(run.Id);
            history.ChangedOn = run.CreatedOn.AddSeconds(-1); await f.Db.SaveChangesAsync();
            assert(!(await store.LoadAsync(run.Id, default))!.RiskReviewApproved, "Stale approval applied");
            history = await f.Db.LeadStatusHistories.SingleAsync(); history.ChangedOn = run.CreatedOn.AddSeconds(1); await f.Db.SaveChangesAsync();
            assert((await store.LoadAsync(run.Id, default))!.RiskReviewApproved, "Approved run not recognized after reload");
        });
        await check("dashboard excludes test/deleted leads, deduplicates deliveries and does not report bids as earnings", async () => {
            await using var f = await ProductionFixture.Create();
            var now = new DateTime(2026, 10, 10, 12, 30, 0, DateTimeKind.Utc);
            f.Lead.CreatedAt = now.AddMinutes(-20); f.Lead.Sales = 999;
            var test = RealLead(); test.Id = 0; test.IsTest = true; test.CreatedAt = now;
            var deleted = RealLead(); deleted.Id = 0; deleted.IsDeleted = true; deleted.CreatedAt = now;
            f.Db.Leads.AddRange(test, deleted); await f.Db.SaveChangesAsync();
            foreach (var id in new[] { f.Lead.Id, test.Id, deleted.Id })
                f.Db.ExternalLeadDeliveries.Add(new() { LeadId = id, PlatformCode = "NETWORX", Status = "Delivered", DeliveredOn = now.AddMinutes(-10) });
            // Historical double-delivery records must not double count one received lead.
            f.Db.ExternalLeadDeliveries.Add(new() { LeadId = f.Lead.Id, PlatformCode = "BLUEINK", Status = "Delivered", DeliveredOn = now.AddMinutes(-10) });
            f.Db.LeadRoutingRuns.Add(new() { LeadId = f.Lead.Id, VerticalCode = "Roofing", RoutingMode = "Auction", Status = "ApprovedRetryPending", CreatedOn = now });
            await f.Db.SaveChangesAsync();
            var report = await MyApp.Api.Services.Reporting.DashboardReport.ReadAsync(f.Db, now, default);
            assert(report.TotalLeads == 1 && report.LeadsToday == 1 && report.DeliveredToday == 1 && report.Pending == 1, "Incorrect live dashboard counts");
            assert(report.Hourly.Count == 24 && report.Hourly.Sum(x => x.Leads) == 1 && report.Hourly.Sum(x => x.Delivered) == 1, "Incorrect hourly buckets");
            assert(report.Providers.Count == 2 && report.Providers.All(x => x.Delivered == 1) && report.RecentRuns.Single().LeadId == f.Lead.Id, "Test/deleted or duplicates leaked");
            assert(report.ConfirmedEarnings == null && report.Cost == null && report.Profit == null, "Fabricated financial data");
            var sql = f.Db.Leads.Where(x => !x.IsTest && !x.IsDeleted).GroupBy(x => new { x.CreatedAt.Date, x.CreatedAt.Hour }).Select(g => g.Count()).ToQueryString();
            assert(sql.Length > 0, "Grouping query failed");
            assert(typeof(DashboardController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles == "SuperAdmin,Admin", "Global dashboard not restricted");
        });
        await check("BLUEINK distinguishes new construction and existing homes and blocks ambiguous installation", async () => {
            foreach (var home in new[] { "Newly constructed home", "Existing home", "", "Unknown" })
            {
                var handler = new CaptureHandler(["{\"status\":\"success\",\"price\":8,\"auth_code\":\"test\"}"]);
                var lead = RealLead();
                lead.AdditionalDataJson = JsonSerializer.Serialize(new { answers = new { projectType = "Install New Roof", roofType = "Metal Roofing", roofInstallationHomeType = home } });
                await BlueInk(handler).PingAsync(AuctionFacts.From(lead, "Roofing"), default);
                if (home is "" or "Unknown") { assert(handler.Bodies.Count == 0, "Ambiguous installation sent"); continue; }
                using var json = JsonDocument.Parse(handler.Bodies.Single());
                var expected = home == "Existing home" ? "New roof for an existing home" : "New roof for new home";
                assert(json.RootElement.GetProperty("data").GetProperty("roof").GetProperty("project_type").GetString() == expected, "Wrong installation mapping");
                assert(!handler.Bodies.Single().Contains(lead.Email!), "Ping exposed contact");
            }
        });
        await check("actual website controller completes an IsTest=false lead through Auction only", async () => {
            await using var f = await ProductionFixture.Create();
            f.Db.LeadTypes.Add(new() { Id = 1, Name = "Roofing", IsActive = true }); await f.Db.SaveChangesAsync();
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["Bouncer:ApiKey"] = "test-only", ["HomeyyWebsiteLeads:LeadTypeIds:roofing"] = "1" }).Build();
            var email = new BouncerEmailValidationService(new HttpClient(new JsonHandler("{\"status\":\"deliverable\",\"score\":99}")), config);
            var controller = new PublicLeadsController(f.Db, new SafeFraud(), new IpLocationService(new HttpClient(new JsonHandler("{}"))),
                new ValidZip(), email, config, f.Router) { ControllerContext = new() { HttpContext = new DefaultHttpContext() } };
            var result = await controller.CreateWebsiteLead(new WebsiteLeadCreateDto {
                IsTest = false, IsCompleted = true, FullName = "Private Person", Email = "private@example.test", Phone = "5551234567",
                Address = "12 Private Street", Postcode = "01007", ServiceCode = "roofing", AdditionalDataJson = f.Lead.AdditionalDataJson });
            assert(result is OkObjectResult, "Real website submission failed: " + JsonSerializer.Serialize((result as ObjectResult)?.Value));
            var run = await f.Db.LeadRoutingRuns.SingleAsync(); var lead = await f.Db.Leads.SingleAsync(x => x.Id == run.LeadId);
            assert(!lead.IsTest && run.RoutingMode == "Auction" && f.Legacy.Calls == 0 && !await f.Db.ExternalLeadDeliveries.AnyAsync(), "Website bypassed auction");
        });
        await check("actual Meta completion and repeated webhook route a real lead once through Auction only", async () => {
            await using var f = await ProductionFixture.Create();
            f.Db.LeadTypes.Add(new() { Id = 1, Name = "Roofing", IsActive = true }); await f.Db.SaveChangesAsync();
            var config = new ConfigurationBuilder().Build();
            var options = Options.Create(new MetaLeadOptions { Enabled = true, GraphApiVersion = "test-version", Pages = new() {
                ["test-page"] = new() { AccessToken = "test-only", PageName = "Roofing", LeadTypeId = 1 } } });
            var service = new MetaLeadService(new HttpClient(new JsonHandler("{\"form_id\":\"test-form\",\"field_data\":[{\"name\":\"full_name\",\"values\":[\"Private Person\"]},{\"name\":\"email\",\"values\":[\"private@example.test\"]},{\"name\":\"zip_code\",\"values\":[\"01007\"]}]}")),
                f.Db, options, new BouncerEmailValidationService(new HttpClient(new JsonHandler("{}")), config), new SafeFraud(), NullLogger<MetaLeadService>.Instance, f.Router);
            await service.ProcessLeadAsync("test-page", "test-form", "test-meta-lead", "{}", default);
            await service.ProcessLeadAsync("test-page", "test-form", "test-meta-lead", "{}", default);
            var run = await f.Db.LeadRoutingRuns.SingleAsync(); var lead = await f.Db.Leads.SingleAsync(x => x.Id == run.LeadId);
            assert(!lead.IsTest && run.RoutingMode == "Auction" && f.Legacy.Calls == 0 && await f.Db.LeadRawPayloads.CountAsync() == 1, "Meta entered legacy or duplicate route");
        });
        await check("Production settings bind validated auction options without credentials", () => {
            var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("MyApp.Api/appsettings.Production.json")).Build();
            var options = config.GetSection("ExternalLeadAuction").Get<ExternalLeadAuctionOptions>()!;
            assert(options.Enabled && options.AuctionWindowSeconds == 90 && ExternalLeadAuctionOptions.Valid(options) && !config.GetValue<bool>("LeadRouting:Enabled"), "Production flags invalid");
            assert(config.GetConnectionString("DefaultConnection") == null, "Production routing settings include a connection string");
            return Task.CompletedTask;
        });
        await check("real completion binds production auction flag and persists only one Auction run", async () => {
            await using var f = await ProductionFixture.Create();
            await f.Router.CompleteAsync(f.Lead);
            await f.Router.CompleteAsync(f.Lead);
            var runs = await f.Db.LeadRoutingRuns.ToListAsync();
            assert(!f.Lead.IsTest && runs.Count == 1 && runs[0].RoutingMode == "Auction", "Real lead entered legacy/duplicate routing");
            assert(f.Legacy.Calls == 0 && !await f.Db.ExternalLeadDeliveries.AnyAsync() && !await f.Db.LeadBiddingResults.AnyAsync(), "Legacy completion or delivery invoked");
            // Production legacy guards are inert even when called directly.
            await new LeadBiddingService(f.Db, f.Auction).RunForLeadAsync(f.Lead.Id);
            await f.Exclusive.QueueAsync(f.Lead, "Roofing", default);
            await f.Exclusive.ProcessAsync(runs[0].Id, default);
            try { await new LeadDeliveryService(f.Db, f.Auction).CreateDeliveryFromBiddingResultAsync(999); throw new Exception("Legacy delivery permitted"); }
            catch (InvalidOperationException) { }
            assert(await f.Db.LeadRoutingRuns.CountAsync() == 1 && !await f.Db.LeadBiddingResults.AnyAsync(), "Direct legacy entry bypassed flag");
            f.Db.ChangeTracker.Clear(); // independent contender must reach the database's unique index
            var conflictingRun = new LeadRoutingRun { LeadId = f.Lead.Id, VerticalCode = "Roofing", RoutingMode = "Legacy", Status = "Pending" };
            f.Db.LeadRoutingRuns.Add(conflictingRun);
            try { await f.Db.SaveChangesAsync(); throw new Exception("Database permitted two routing runs"); }
            catch (DbUpdateException) { f.Db.Entry(conflictingRun).State = EntityState.Detached; }
            assert(await f.Db.LeadRoutingRuns.CountAsync() == 1, "Unique lead ownership index missing");
        });
        await check("real covered completion retains Thumbtack ownership and never opens an external auction", async () => {
            await using var f = await ProductionFixture.Create();
            f.Db.ThumbtackCoverageServices.Add(new() { ServiceCode = "Roofing", CategoryPk = "test-category", IsEnabled = true });
            f.Db.ThumbtackZipCoverages.Add(new() { ServiceCode = "Roofing", ZipCode = "01007", IsEnabled = true });
            await f.Db.SaveChangesAsync();
            await f.Router.CompleteAsync(f.Lead);
            var run = await f.Db.LeadRoutingRuns.SingleAsync();
            var store = new MemoryStore(new(run, f.Lead, []));
            var gateway = new FakeGateway([("BLUEINK", 8.32m, 0)]);
            await new ExternalAuctionEngine(store, gateway, new CoverageGateway(f.Db), f.Auction).ProcessAsync(run.Id, default);
            await f.Db.SaveChangesAsync();
            assert(run.WinnerPlatformCode == "THUMBTACK" && run.Status == "ThumbtackSelected" && run.RoutingMode != "Legacy", "Covered lead ownership missing");
            assert(run.AuctionStartedOn == null && gateway.Pings.Count == 0 && gateway.Posts.Count == 0 && f.Legacy.Calls == 0, "Covered lead entered auction/legacy");
        });
        await check("uncovered real completion collects BLUEINK 8.32 bid and locks winner before posting; unconfigured Modernize receives no ping", async () => {
            await using var f = await ProductionFixture.Create();
            await f.Router.CompleteAsync(f.Lead);
            var run = await f.Db.LeadRoutingRuns.SingleAsync();
            var store = new MemoryStore(new(run, f.Lead, [])) { Rules = [
                new() { Id = 1, PlatformCode = "BLUEINK" }, new() { Id = 2, PlatformCode = "Modernize" }] };
            var handler = new WinnerCheckingHandler(store, assert);
            var provider = BlueInk(handler);
            var gateway = new BlueGateway(provider);
            await new ExternalAuctionEngine(store, gateway, new CoverageGateway(f.Db), f.Auction).ProcessAsync(run.Id, default);
            await f.Db.SaveChangesAsync();
            assert(run.Status == "Won" && run.RoutingMode == "Auction" && run.AuctionStartedOn != null, "Real lead did not finish auction");
            assert(store.Work.Attempts.Count == 2 && store.Work.Attempts[0].PlatformCode == "BLUEINK" && store.Work.Attempts[0].OfferedBidAmount == 8.32m, "Wrong bid/Modernize attempt");
            assert(handler.Requests == 2 && store.WinnerCommits == 1 && f.Legacy.Calls == 0, "Wrong send sequence");
            await f.Router.CompleteAsync(f.Lead);
            assert(await f.Db.LeadRoutingRuns.CountAsync() == 1, "Repeat completion created second system");
        });
        await check("BLUEINK maps nested Replace Roof and Metal Roofing using configured approved values", async () => {
            var handler = new CaptureHandler(["{\"status\":\"success\",\"price\":\"8.32\",\"auth_code\":\"token\"}"]);
            var lead = RealLead();
            await BlueInk(handler).PingAsync(AuctionFacts.From(lead, "Roofing"), default);
            using var json = JsonDocument.Parse(handler.Bodies.Single());
            var roof = json.RootElement.GetProperty("data").GetProperty("roof");
            assert(roof.GetProperty("project_type").GetString() == "New roof for an existing home" &&
                roof.GetProperty("roofing_type").GetString() == "Metal", "Nested roofing mapping lost");
            // Approved values supplied from BLUEINK documentation; unmapped inputs remain blocked.
            var unmapped = new BlueInkLeadProvider(new HttpClient(handler), Options.Create(new BlueInkOptions {
                Verticals = new() { ["Roofing"] = new() { Token = "TEST-ONLY" } }
            }), NullLogger<BlueInkLeadProvider>.Instance);
            var offer = await unmapped.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
            assert(!offer.Accepted && handler.Bodies.Count == 1, "Empty or guessed roof was sent");
        });
        await check("documented BLUEINK production mappings cover public roofing materials and repair on ping and post", async () => {
            foreach (var pair in new[] { ("Asphalt Shingles", "Asphalt shingle"), ("Metal Roofing", "Metal"), ("Tile Roofing", "Tile"), ("cedar Shake", "Cedar shake"), ("Tar Roofing", "Tar"), ("Natural Slate", "Natural slate") }) {
                var handler = new CaptureHandler(["{\"status\":\"success\",\"price\":\"8.32\",\"auth_code\":\"token\"}", "{\"status\":\"success\",\"confirmation_id\":\"test\"}"]);
                var lead = RealLead();
                lead.AdditionalDataJson = JsonSerializer.Serialize(new { answers = new { projectType = "Repair Roof", roofType = pair.Item1 } });
                var provider = BlueInk(handler);
                await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
                await provider.PostAsync(lead, "Roofing", "token", null, "test-only", default);
                assert(handler.Bodies.Count == 2, "Ping/post not sent for approved mapping");
                foreach (var body in handler.Bodies) {
                    using var json = JsonDocument.Parse(body);
                    var roof = json.RootElement.GetProperty("data").GetProperty("roof");
                    assert(roof.GetProperty("project_type").GetString() == "Repair" && roof.GetProperty("roofing_type").GetString() == pair.Item2, "Incorrect documented mapping");
                }
            }
        });
        await check("production gateway refuses unconfigured Modernize", async () => {
            var services = new ServiceCollection().AddDataProtection().Services.BuildServiceProvider();
            using (services) {
                var gateway = new AuctionGateway(services.GetRequiredService<IServiceScopeFactory>(), services.GetRequiredService<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>());
                assert(!gateway.Enabled("Modernize", "Roofing"), "Modernize enabled");
                try { await gateway.PingAsync("modernize", AuctionFacts.From(RealLead(), "Roofing"), default); throw new Exception("Modernize sent"); }
                catch (InvalidOperationException) { }
            }
        });
        await check("prior Legacy run or external delivery prevents auction replay for a real lead", async () => {
            await using var f = await ProductionFixture.Create();
            f.Db.LeadRoutingRuns.Add(new() { LeadId = f.Lead.Id, VerticalCode = "Roofing", RoutingMode = "Legacy", Status = "Won", RowVersion = [] });
            await f.Db.SaveChangesAsync();
            await f.Router.CompleteAsync(f.Lead);
            assert(await f.Db.LeadRoutingRuns.CountAsync() == 1 && (await f.Db.LeadRoutingRuns.SingleAsync()).RoutingMode == "Legacy" && f.Legacy.Calls == 0, "Existing delivered lead reauctioned");
        });
        await check("prior external post prevents creating a fresh auction for a real lead", async () => {
            await using var f = await ProductionFixture.Create();
            f.Db.ExternalLeadDeliveries.Add(new() { LeadId = f.Lead.Id, PlatformCode = "BLUEINK", VerticalCode = "Roofing", Status = "Delivered" });
            await f.Db.SaveChangesAsync();
            await f.Router.CompleteAsync(f.Lead);
            assert(!await f.Db.LeadRoutingRuns.AnyAsync() && f.Legacy.Calls == 0 && await f.Db.ExternalLeadDeliveries.CountAsync() == 1, "Previously posted lead reauctioned");
        });
    }

    private static Lead RealLead() {
        var lead = Fixture.Lead(); lead.IsTest = false; lead.LeadTypeId = null; lead.CampaignName = "Roofing";
        lead.AdditionalDataJson = "{\"answers\":{\"projectType\":\"Replace Roof\",\"roofType\":\"Metal Roofing\"}}";
        return lead;
    }
    internal static BlueInkLeadProvider BlueInk(HttpMessageHandler handler)
    {
        var config = new ConfigurationBuilder().AddJsonFile(Path.GetFullPath("MyApp.Api/appsettings.Production.json")).Build();
        var options = config.GetSection(BlueInkOptions.SectionName).Get<BlueInkOptions>()!;
        options.Enabled = true; options.UseTestEndpoints = false;
        options.Verticals = new(StringComparer.OrdinalIgnoreCase) { ["Roofing"] = new() { Enabled = true, Token = "TEST-ONLY" } };
        return new(new HttpClient(handler), Options.Create(options), NullLogger<BlueInkLeadProvider>.Instance);
    }
    private sealed class ProductionFixture : IAsyncDisposable
    {
        public required SqliteConnection Connection;
        public required IntegrationDb Db;
        public required Lead Lead;
        public required CompletedLeadRouter Router;
        public required ExclusiveLeadRoutingService Exclusive;
        public required IOptions<ExternalLeadAuctionOptions> Auction;
        public LegacySpy Legacy = new();
        public static async Task<ProductionFixture> Create() {
            var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
            var db = new IntegrationDb(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlite(connection).Options);
            await db.Database.EnsureCreatedAsync();
            var lead = RealLead(); db.Leads.Add(lead); await db.SaveChangesAsync();
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
                ["ExternalLeadAuction:Enabled"] = "true", ["ExternalLeadAuction:KeyRingPath"] = Path.GetFullPath("test-key-ring"),
                ["LeadRouting:Enabled"] = "false" }).Build();
            var services = new ServiceCollection(); services.AddOptions<ExternalLeadAuctionOptions>().Bind(configuration.GetSection("ExternalLeadAuction"));
            using var sp = services.BuildServiceProvider(); var auction = sp.GetRequiredService<IOptions<ExternalLeadAuctionOptions>>();
            var exclusiveOptions = Options.Create(new ExclusiveRoutingOptions { Enabled = true }); // stale legacy option must still lose to auction
            var exclusive = new ExclusiveLeadRoutingService(db, [], exclusiveOptions, NullLogger<ExclusiveLeadRoutingService>.Instance, auction);
            var queue = new ExternalLeadDistributionService(db, NullLogger<ExternalLeadDistributionService>.Instance, exclusive, exclusiveOptions, auction, new TestLocker());
            var legacy = new LegacySpy();
            return new() { Connection = connection, Db = db, Lead = lead, Auction = auction, Exclusive = exclusive, Legacy = legacy,
                Router = new CompletedLeadRouter(queue, legacy, auction, exclusiveOptions) };
        }
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await Connection.DisposeAsync(); }
    }
    private sealed class LegacySpy : ILegacyLeadCompletion { public int Calls; public Task CompleteAsync(long id, CancellationToken ct) { Calls++; return Task.CompletedTask; } }
    private sealed class SafeFraud : ILeadFraudService { public Task CheckAndApplyAsync(Lead lead) { lead.FraudScore = 0; lead.IsSuspicious = false; return Task.CompletedTask; } }
    private sealed class ValidZip : IAddressValidationService { public Task<VerifyAddressPostcodeResponse> VerifyAsync(VerifyAddressPostcodeRequest model) => Task.FromResult(new VerifyAddressPostcodeResponse { IsValid = true, Status = "Valid", PostalCode = model.Postcode, Country = "US", City = "Test City", State = "WA" }); }
    private sealed class JsonHandler(string json) : HttpMessageHandler { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent(json) }); }
    private sealed class TestLocker : ILeadExecutionLocker {
        private readonly SemaphoreSlim mutex = new(1);
        public async Task<IAsyncDisposable?> AcquireAsync(MyAppDbContext db, long id, CancellationToken ct) { await mutex.WaitAsync(ct); return new Unlock(mutex); }
        private sealed class Unlock(SemaphoreSlim gate) : IAsyncDisposable { public ValueTask DisposeAsync() { gate.Release(); return ValueTask.CompletedTask; } }
    }
    private sealed class CoverageGateway(MyAppDbContext db) : IThumbtackAuctionGateway {
        public Task<bool> IsCoveredAsync(AuctionWork w, CancellationToken ct) => ThumbtackCoverageLookup.HasCoverageAsync(db, w.Run.VerticalCode, w.Lead.Postcode, ct);
        public Task<ThumbtackCheck> CheckAsync(AuctionWork w, CancellationToken ct) => Task.FromResult(new ThumbtackCheck(true, false, "No professionals", "saved", "{}"));
    }
    private sealed class BlueGateway(BlueInkLeadProvider provider) : IAuctionGateway {
        public bool Enabled(string p, string v) => p == "BLUEINK"; public bool Idempotent(string p) => false;
        public string Protect(string s) => s; public string Unprotect(string s) => s;
        public Task<AuctionOffer> PingAsync(string p, AuctionFacts f, CancellationToken ct) => provider.PingAsync(f, ct);
        public Task<AuctionPost> PostAsync(string p, AuctionWork w, string reference, string? context, string key, CancellationToken ct) => provider.PostAsync(w.Lead, w.Run.VerticalCode, reference, context, key, ct);
    }
    private sealed class WinnerCheckingHandler(MemoryStore store, Action<bool,string> assert) : HttpMessageHandler {
        public int Requests;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) {
            var body = await request.Content!.ReadAsStringAsync(ct); Requests++;
            if (Requests == 1) {
                assert(request.RequestUri!.AbsolutePath == "/ping" && store.Work.Run.WinnerAttemptId == null && store.WinnerCommits == 0, "Post before auction/wrong production endpoint");
                foreach (var secret in new[] { store.Work.Lead.Email!, store.Work.Lead.Phone!, store.Work.Lead.FullName!, store.Work.Lead.Address! }) assert(!body.Contains(secret), "Ping PII leak");
            } else {
                assert(request.RequestUri!.AbsolutePath == "/post" && store.WinnerCommits == 1 && store.Work.Attempts.Single(a => a.IsWinner).PlatformCode == "BLUEINK" && body.Contains(store.Work.Lead.Email!), "Full contact sent before winner lock");
            }
            return new(System.Net.HttpStatusCode.OK) { Content = new StringContent(Requests == 1 ? "{\"status\":\"success\",\"price\":\"8.32\",\"auth_code\":\"test-token\"}" : "{\"status\":\"success\",\"confirmation_id\":\"test-delivery\"}") };
        }
    }
    // Keep the actual EF entities, indexes, relationships and queue queries. Only adapt
    // SQL Server physical column/default syntax to isolated SQLite; no application host starts.
    private sealed class IntegrationDb(DbContextOptions<MyAppDbContext> options) : MyAppDbContext(options) {
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) foreach (var property in entity.GetProperties()) {
                property.SetColumnType(null); property.SetDefaultValueSql(null); property.SetComputedColumnSql(null);
                if (property.IsConcurrencyToken && property.ClrType == typeof(byte[])) {
                    property.ValueGenerated = ValueGenerated.OnAdd;
                    property.SetDefaultValueSql("X'00'");
                }
            }
        }
    }
}
