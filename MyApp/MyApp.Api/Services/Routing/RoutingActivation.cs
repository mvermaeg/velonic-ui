namespace MyApp.Api.Services.Routing;

public static class RoutingActivation
{
    public static bool CanCreateAuction(bool existingRun, bool previousExternalDelivery) => !existingRun && !previousExternalDelivery;
    public static string Mode(bool auction, bool exclusive) => auction ? "Auction" : exclusive ? "LegacyExclusive" : "LegacyDistribution";
}
