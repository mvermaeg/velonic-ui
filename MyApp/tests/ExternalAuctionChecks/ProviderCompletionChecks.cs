using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
using MyApp.Api.Services.ExternalDeliveries.Modernize;
using MyApp.Api.Services.ExternalDeliveries.Networx;

static class ProviderCompletionChecks
{
    public static async Task Run(Func<string, Func<Task>, Task> check, Action<bool, string> assert)
    {
        await check("external auction ignores geographic filters but retains dates and activation", () => {
            var now = DateTime.UtcNow;
            var rule = new LeadRoutingRule { IsActive = true, DestinationType = "ExternalPlatform", VerticalCode = "Roofing", State = "TEXAS", Postcode = "78701" };
            var eligible = AuctionRulePolicy.Eligible("Roofing", now).Compile();
            assert(eligible(rule), "ZIP/state still filter external auction");
            rule.IsActive = false; assert(!eligible(rule), "Disabled rule included"); rule.IsActive = true;
            rule.EffectiveTo = now.AddSeconds(-1); assert(!eligible(rule), "Expired rule included"); rule.EffectiveTo = null;
            rule.EffectiveFrom = now.AddDays(1); assert(!eligible(rule), "Future rule included"); rule.EffectiveFrom = null;
            rule.DestinationType = "InternalClient"; assert(!eligible(rule), "Internal rule included");
            using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
            var sql = db.LeadRoutingRules.Where(AuctionRulePolicy.Eligible("Roofing", now)).Select(x => x.Id).ToQueryString();
            assert(!sql.Contains("[Postcode]") && !sql.Contains("[State]"), "SQL retains geography filter");
            return Task.CompletedTask;
        });
        await check("all configured adapters get defaults without overriding disabled rules or caps", () => {
            var gateway = new FakeGateway([]);
            var existing = new[] { new LeadRoutingRule { PlatformCode = "BLUEINK", VerticalCode = "Roofing", DestinationType = "ExternalPlatform", IsActive = false, DailyCap = 1 } };
            var missing = AuctionRulePolicy.MissingProviders(existing, "Roofing", gateway).ToArray();
            assert(!missing.Contains("BLUEINK") && missing.Contains("Modernize") && missing.Contains("NETWORX"), "Provider enumeration or disabled override");
            gateway.Disabled = true;
            assert(!AuctionRulePolicy.MissingProviders([], "Roofing", gateway).Any(), "Unconfigured adapters defaulted");
            return Task.CompletedTask;
        });
        await check("NETWORX metal replacement uses its own approved mapping and never asphalt", async () => {
            using var db = new MyAppDbContext(new DbContextOptionsBuilder<MyAppDbContext>().UseSqlServer("Server=offline.invalid;Database=NeverOpened;Integrated Security=true").Options);
            var lead = MetalLead(); var facts = AuctionFacts.From(lead, "Roofing");
            assert(NetworxLeadProvider.AuctionTaskOption(facts) == "ROOF_REPLACE_METAL", "Wrong material classification");
            var handler = new CaptureHandler(["<response><statusCode>200</statusCode><token>net-token</token><price>42.50</price></response>"]);
            var lookup = new RecordingTaskLookup();
            var provider = new NetworxLeadProvider(new HttpClient(handler), db, Options.Create(new NetworxOptions { Enabled = true, UserId = "test", AccessKey = "test" }), NullLogger<NetworxLeadProvider>.Instance, lookup);
            var rejected = await provider.PingAsync(facts, default);
            assert(!rejected.Accepted && rejected.Error!.Contains("ROOF_REPLACE_METAL") && handler.Bodies.Count == 0, "Missing metal task silently substituted");
            lookup.Tasks = ["approved-test-metal-id"];
            var accepted = await provider.PingAsync(facts, default);
            assert(accepted.Accepted && lookup.Option == "ROOF_REPLACE_METAL" && accepted.PostContext == "approved-test-metal-id", "Approved metal mapping unused");
        });
        await check("Modernize contact-free ping parses price/token and bounded expiry", async () => {
            var handler = new CaptureHandler(["{\"status\":\"success\",\"pingToken\":\"test-token\",\"price\":\"50\"}"]);
            var provider = Modernize(handler); var lead = MetalLead(); var started = DateTime.UtcNow;
            var offer = await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
            assert(offer.Accepted && offer.Amount == 50 && offer.Reference == "test-token", "Documented bid not parsed");
            assert(offer.ExpiresOn >= started.AddMinutes(29) && offer.ExpiresOn <= DateTime.UtcNow.AddMinutes(30), "Wrong expiry");
            using var payload = JsonDocument.Parse(handler.Bodies.Single());
            assert(payload.RootElement.GetProperty("service").GetString() == "ROOFING_METAL", "Metal roof misclassified");
            assert(payload.RootElement.GetProperty("postalCode").GetString() == "01007", "ZIP changed");
            foreach (var secret in new[] { lead.FullName!, lead.Email!, lead.Phone!, lead.Address!, lead.TrustedFormCertificateUrl!, lead.TcpaComplianceText! })
                assert(!handler.Bodies.Single().Contains(secret), "Ping leaked contact/compliance");
            assert(!offer.RequestAudit.Contains("test-tag") && !offer.ResponseAudit.Contains("test-token"), "Audit leaked credentials");
        });
        await check("Modernize posts contacts only on separate winner call and preserves ping snapshot", async () => {
            var handler = new CaptureHandler(["{\"status\":\"success\",\"pingToken\":\"test-token\",\"price\":50}", "{\"status\":\"success\",\"leadId\":\"provider-lead\"}"]);
            var provider = Modernize(handler); var lead = MetalLead(); lead.State = "South Dakota";
            var offer = await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
            assert(handler.Bodies.Count == 1, "Ping automatically posted");
            lead.AdditionalDataJson = "{}";
            var post = await provider.PostAsync(lead, "Roofing", offer.Reference!, offer.PostContext, "key", default);
            assert(post.Accepted && post.DeliveryId == "provider-lead" && handler.Bodies.Count == 2, "Winner post failed");
            using var payload = JsonDocument.Parse(handler.Bodies[1]); var root = payload.RootElement;
            assert(root.GetProperty("service").GetString() == "ROOFING_METAL" && root.GetProperty("state").GetString() == "SD", "Snapshot/state lost");
            assert(root.GetProperty("email").GetString() == lead.Email && root.GetProperty("pingToken").GetString() == "test-token", "Contact/token missing");
            assert(!post.RequestAudit.Contains(lead.Email!), "Post audit leaked contact");
        });
        foreach (var response in new[] { "{}", "not-json", "{\"status\":\"success\"}" })
            await check("Modernize ambiguous post requires reconciliation: " + response, async () => {
                var handler = new CaptureHandler(["{\"status\":\"success\",\"price\":20,\"pingToken\":\"token\"}", response]);
                var provider = Modernize(handler); var lead = MetalLead();
                var offer = await provider.PingAsync(AuctionFacts.From(lead, "Roofing"), default);
                var post = await provider.PostAsync(lead, "Roofing", "token", offer.PostContext, "key", default);
                assert(!post.Accepted && post.OutcomeUnknown && !post.RetrySafe, "Ambiguous success or unsafe retry");
            });
        await check("Modernize rejected malformed and invalid bids cannot win", async () => {
            foreach (var response in new[] { "{\"status\":\"rejected\",\"price\":50,\"pingToken\":\"token\"}", "{\"status\":\"success\",\"price\":0,\"pingToken\":\"token\"}", "{\"status\":\"success\",\"price\":50}", "not-json" }) {
                var offer = await Modernize(new CaptureHandler([response])).PingAsync(AuctionFacts.From(MetalLead(), "Roofing"), default);
                var now = DateTime.UtcNow;
                assert(AuctionPolicy.Reject(offer, now, now.AddSeconds(90), now) != null, "Invalid offer won");
            }
        });
        await check("Modernize does not invent missing roofing qualifications", async () => {
            var handler = new CaptureHandler(["unused"]); var lead = MetalLead(); lead.AdditionalDataJson = "{}";
            var offer = await Modernize(handler).PingAsync(AuctionFacts.From(lead, "Roofing"), default);
            assert(!offer.Accepted && handler.Bodies.Count == 0, "Fabricated roof sent");
        });
        await check("Modernize handles shared LeadTypeId using actual vertical and window ranges", () => {
            var lead = MetalLead(); lead.LeadTypeId = 2;
            lead.AdditionalDataJson = "{\"answers\":{\"projectType\":\"Repair window\",\"windowCount\":\"10+\"}}";
            var windows = ModernizeLeadProvider.AuctionPayload(AuctionFacts.From(lead, "Windows"))!;
            assert((string)windows["WindowsProjectScope"]! == "Repair" && (string)windows["NumberOfWindows"]! == "6-9", "Window range/project mapping");
            var bath = ModernizeLeadProvider.AuctionPayload(AuctionFacts.From(lead, "Bathroom"))!;
            assert((string)bath["service"]! == "BATH_REMODEL", "Shared lead type misclassified bathroom");
            return Task.CompletedTask;
        });
    }
    private static Lead MetalLead() { var lead = Fixture.Lead(); lead.AdditionalDataJson = "{\"answers\":{\"projectType\":\"Replace Roof\",\"roofType\":\"Metal Roofing\"},\"purchaseTimeFrame\":\"Immediately\"}"; return lead; }
    private static ModernizeLeadProvider Modernize(CaptureHandler handler) => new(new HttpClient(handler),
        Options.Create(new ModernizeOptions { Enabled = true, UseStaging = true, StagingTagId = "test-tag" }), NullLogger<ModernizeLeadProvider>.Instance);
    private sealed class RecordingTaskLookup : INetworxAuctionTaskLookup {
        public List<string> Tasks = []; public string? Option;
        public Task<List<string>> FindAsync(int leadTypeId, string option, CancellationToken ct) { Option = option; return Task.FromResult(Tasks); }
    }
}
