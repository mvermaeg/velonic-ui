using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Services.Reporting;

public sealed record DashboardBucket(DateTime HourUtc, int Leads, int Delivered);
public sealed record DashboardProvider(string Platform, int Delivered);
public sealed record DashboardRun(long LeadId, string Vertical, string Mode, string Status, string? Winner, DateTime CreatedOn);
public sealed record DashboardSummary(DateTime AsOfUtc, string Timezone, int TotalLeads, int LeadsToday,
    int DeliveredToday, int Pending, int ThumbtackOnly, int NoBid, int NeedsAttention,
    decimal? ConfirmedEarnings, decimal? Cost, decimal? Profit, string FinancialNote,
    IReadOnlyList<DashboardBucket> Hourly, IReadOnlyList<DashboardProvider> Providers, IReadOnlyList<DashboardRun> RecentRuns);

public static class DashboardReport
{
    public static async Task<DashboardSummary> ReadAsync(MyAppDbContext db, DateTime now, CancellationToken ct)
    {
        var today = now.Date;
        var from = today.AddDays(-29);
        var startHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).AddHours(-23);
        var leads = db.Leads.AsNoTracking().Where(x => !x.IsDeleted && !x.IsTest);
        var total = await leads.CountAsync(ct);
        var leadsToday = await leads.CountAsync(x => x.CreatedAt >= today && x.CreatedAt <= now, ct);
        var external = db.ExternalLeadDeliveries.AsNoTracking().Where(x => x.Status == "Delivered" &&
            x.DeliveredOn != null && x.DeliveredOn <= now && leads.Any(l => l.Id == x.LeadId));
        var internalDeliveries = db.LeadDeliveries.AsNoTracking().Where(x => x.DeliveryStatus == "Delivered" &&
            x.DeliveredOn != null && x.DeliveredOn <= now && leads.Any(l => l.Id == x.LeadId));
        var deliveredToday = await external.Where(x => x.DeliveredOn >= today).Select(x => x.LeadId)
            .Union(internalDeliveries.Where(x => x.DeliveredOn >= today).Select(x => x.LeadId)).CountAsync(ct);
        var runs = db.LeadRoutingRuns.AsNoTracking().Where(x => leads.Any(l => l.Id == x.LeadId));
        var pending = await runs.CountAsync(x => x.Status == "Pending" || x.Status == "ApprovedRetryPending" || x.Status == "CheckingThumbtack" ||
            x.Status == "Pinging" || x.Status == "CollectingBids" || x.Status == "WinnerSelected" || x.Status == "Posting", ct);
        var recent = runs.Where(x => x.CreatedOn >= from && x.CreatedOn <= now);
        var thumbtack = await recent.CountAsync(x => x.ThumbtackSearchStartedOn != null || x.Status == "ThumbtackSelected" || x.Status == "ThumbtackFailed", ct);
        var noBid = await recent.CountAsync(x => x.Status == "NoBid", ct);
        var attention = await runs.CountAsync(x => x.Status == "Failed" || x.Status == "ThumbtackFailed" || x.Status == "ReconciliationRequired", ct);
        var hourlyLeads = await leads.Where(x => x.CreatedAt >= startHour && x.CreatedAt <= now)
            .GroupBy(x => new { x.CreatedAt.Date, x.CreatedAt.Hour }).Select(g => new { g.Key.Date, g.Key.Hour, Count = g.Count() }).ToListAsync(ct);
        var deliveries = external.Where(x => x.DeliveredOn >= startHour).Select(x => new { x.LeadId, At = x.DeliveredOn!.Value })
            .Union(internalDeliveries.Where(x => x.DeliveredOn >= startHour).Select(x => new { x.LeadId, At = x.DeliveredOn!.Value }));
        var hourlyDelivered = await deliveries.GroupBy(x => new { x.At.Date, x.At.Hour })
            .Select(g => new { g.Key.Date, g.Key.Hour, Count = g.Select(x => x.LeadId).Distinct().Count() }).ToListAsync(ct);
        var buckets = Enumerable.Range(0, 24).Select(i => {
            var hour = startHour.AddHours(i);
            return new DashboardBucket(hour, hourlyLeads.FirstOrDefault(x => x.Date == hour.Date && x.Hour == hour.Hour)?.Count ?? 0,
                hourlyDelivered.FirstOrDefault(x => x.Date == hour.Date && x.Hour == hour.Hour)?.Count ?? 0);
        }).ToArray();
        var providers = await external.Where(x => x.DeliveredOn >= from).GroupBy(x => x.PlatformCode)
            .OrderByDescending(g => g.Select(x => x.LeadId).Distinct().Count())
            .Select(g => new DashboardProvider(g.Key, g.Select(x => x.LeadId).Distinct().Count())).ToListAsync(ct);
        var latest = await runs.OrderByDescending(x => x.CreatedOn).ThenByDescending(x => x.Id).Take(20)
            .Select(x => new DashboardRun(x.LeadId, x.VerticalCode, x.RoutingMode, x.Status, x.WinnerPlatformCode, x.CreatedOn)).ToListAsync(ct);
        return new(now, "UTC", total, leadsToday, deliveredToday, pending, thumbtack, noBid, attention,
            null, null, null, "Provider earnings and acquisition costs are not reconciled in this database. Bids and accepted deliveries are not confirmed earnings.",
            buckets, providers, latest.Select(x => x with { CreatedOn = DateTime.SpecifyKind(x.CreatedOn, DateTimeKind.Utc) }).ToArray());
    }
}
