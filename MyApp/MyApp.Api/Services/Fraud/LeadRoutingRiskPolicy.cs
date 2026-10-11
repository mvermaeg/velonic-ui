using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Fraud;

public static class LeadRoutingRiskPolicy
{
    public const int MinimumExclusiveQualityScore = 60;

    // IsSuspicious remains an administrative risk indicator. The numerical
    // cutoff controls automatic routing, including previously scored leads.
    public static bool Blocks(Lead lead) => !lead.IsTest &&
        (lead.IsInvalidEmail || lead.LeadQualityScore <= MinimumExclusiveQualityScore);
}
