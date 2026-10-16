using MyApp.Api.Data;

namespace MyApp.Api.Services.Routing;

// Keep storage, list display, filters and historical records consistent.
public static class LeadRoutingStatus
{
    public static string FromRun(string status) => status == "Won" ? "Sold" : status;

    public static IQueryable<LeadStatusRow> Read(MyAppDbContext db) =>
        from lead in db.Leads
        where !lead.IsDeleted
        let status = db.LeadRoutingRuns.Where(r => r.LeadId == lead.Id && r.RoutingMode == "Auction")
            .OrderByDescending(r => r.Id).Select(r => r.Status).FirstOrDefault()
        select new LeadStatusRow { LeadId = lead.Id, Status = status == null ? lead.LeadStatus : status == "Won" ? "Sold" : status };
}

public sealed class LeadStatusRow
{
    public long LeadId { get; init; }
    public string? Status { get; init; }
}
