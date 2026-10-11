using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Fraud;

public static class LeadRoutingRiskPolicy
{
    public const int MaximumAutomaticFraudScore = 60;

    // IsSuspicious remains an administrative risk indicator. The numerical
    // cutoff controls automatic routing, including previously scored leads.
    // Quality is 100 minus fraud score; do not impose a second quality >60
    // gate that would incorrectly block the approved fraud-score range 40-60.
    public static bool Blocks(Lead lead, bool approvedForThisRun = false) => !lead.IsTest &&
        (lead.IsInvalidEmail || lead.FraudScore > MaximumAutomaticFraudScore && !approvedForThisRun);

    public static string ApprovalRemark(long runId) =>
        $"Routing risk approval: run={runId}; user-confirmed distinct genuine request.";
}
