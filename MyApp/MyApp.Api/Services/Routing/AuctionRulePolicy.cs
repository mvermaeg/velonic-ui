using System.Linq.Expressions;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Routing;

public static class AuctionRulePolicy
{
    // Geography determines Thumbtack ownership only. External buyers decide coverage
    // from the ZIP in their ping, not from legacy internal bidding-rule filters.
    public static Expression<Func<LeadRoutingRule, bool>> Eligible(string vertical, DateTime now) =>
        x => x.IsActive && x.DestinationType == "ExternalPlatform" && x.VerticalCode == vertical &&
            (x.EffectiveFrom == null || x.EffectiveFrom <= now) && (x.EffectiveTo == null || x.EffectiveTo > now);

    public static IEnumerable<string> MissingProviders(IEnumerable<LeadRoutingRule> existing,
        string vertical, IAuctionGateway gateway) => AuctionGateway.Capabilities
        .Where(x => x.MonetaryBid && gateway.Enabled(x.PlatformCode, vertical) &&
            !existing.Any(r => r.DestinationType == "ExternalPlatform" && r.VerticalCode == vertical &&
                string.Equals(r.PlatformCode, x.PlatformCode, StringComparison.OrdinalIgnoreCase)))
        .Select(x => x.PlatformCode);
}
