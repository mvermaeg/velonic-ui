using Microsoft.Extensions.Options;
using MyApp.Api.Services.ExternalDeliveries.Networx;

namespace MyApp.Api.Services.Routing;

// Configuration-only startup checks. Never opens a database or calls a provider.
public sealed class AuctionConfigurationCheck(IOptions<ExternalLeadAuctionOptions> auction,
    IOptions<NetworxOptions> networx, IServiceScopeFactory scopes, ILogger<AuctionConfigurationCheck> logger) : IHostedService
{
    public Task StartAsync(CancellationToken ct)
    {
        if (!auction.Value.Enabled) return Task.CompletedTask;
        using var scope = scopes.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IAuctionGateway>();
        foreach (var provider in AuctionGateway.Capabilities.Where(x => x.MonetaryBid))
        {
            var verticals = new[] { "Roofing", "Windows", "Bathroom", "HVAC", "Gutters" }.Where(v => gateway.Enabled(provider.PlatformCode, v)).ToArray();
            if (verticals.Length == 0)
                logger.LogWarning("Auction provider {Provider} is unavailable: disabled, unsupported verticals, missing credentials, invalid HTTPS endpoints or split ping/post mode disabled. Check configuration; values redacted.", provider.PlatformCode);
        }
        logger.LogInformation("Modernize uses the documented price/pingToken contract. Production activation requires provider staging approval. Unknown winner post outcomes require reconciliation; automatic replay is disabled.");
        if (networx.Value.Enabled)
        {
            var n = networx.Value;
            if (!n.UsePingPost || string.IsNullOrWhiteSpace(n.UserId) || string.IsNullOrWhiteSpace(n.AccessKey) ||
                !Uri.TryCreate(n.BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                throw new OptionsValidationException("Networx", typeof(NetworxOptions),
                    ["Auction Networx requires ping/post mode, credentials and an absolute HTTPS BaseUrl; values redacted."]);
            logger.LogWarning("NETWORX readiness requires approved provider task IDs for each project option. Check /api/lead-routing/networx-readiness for missing/ambiguous mappings, including roofing operation and material. Gutters is unsupported. Each lead is checked before HTTP; startup does not assume database mappings exist.");
        }
        return Task.CompletedTask;
    }
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
