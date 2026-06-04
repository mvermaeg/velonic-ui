using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly MyAppDbContext _db;

    public ReportsController(MyAppDbContext db)
    {
        _db = db;
    }

    [HttpGet("general")]
    public async Task<IActionResult> General()
    {
        var totalLeads = await _db.Leads.CountAsync();

        var soldLeads = await _db.Leads
            .CountAsync(x => x.LeadStatus == "Sold");

        var deliveredLeads = await _db.LeadDeliveries.CountAsync();

        var totalSales = await _db.Leads
            .SumAsync(x => (decimal?)x.Sales) ?? 0;

        var totalProfit = await _db.Leads
            .SumAsync(x => (decimal?)x.Profit) ?? 0;

        var totalClients = await _db.Clients
            .CountAsync(x => !x.IsDeleted);

        return Ok(new
        {
            totalLeads,
            soldLeads,
            deliveredLeads,
            totalSales,
            totalProfit,
            totalClients
        });
    }

    [HttpGet("campaign")]
    public async Task<IActionResult> Campaign()
    {
        var data = await _db.Leads
            .GroupBy(x => x.CampaignName)
            .Select(g => new
            {
                campaign = g.Key,
                leads = g.Count(),
                sold = g.Count(x => x.LeadStatus == "Sold"),
                sales = g.Sum(x => x.Sales ?? 0),
                profit = g.Sum(x => x.Profit ?? 0)
            })
            .OrderByDescending(x => x.leads)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("vendors/daily")]
    public async Task<IActionResult> VendorDaily()
    {
        var data = await _db.Leads
            .GroupBy(x => x.CreatedAt.Date)
            .Select(g => new
            {
                reportDate = g.Key,
                leads = g.Count(),
                sales = g.Sum(x => x.Sales ?? 0),
                profit = g.Sum(x => x.Profit ?? 0)
            })
            .OrderByDescending(x => x.reportDate)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("vendors/subid")]
    [HttpGet("vendors/sub-id")]
    public async Task<IActionResult> VendorSubId()
    {
        var data = await _db.Leads
            .GroupBy(x => x.AffiliateSubId)
            .Select(g => new
            {
                subId = g.Key,
                leads = g.Count(),
                sales = g.Sum(x => x.Sales ?? 0),
                profit = g.Sum(x => x.Profit ?? 0)
            })
            .OrderByDescending(x => x.leads)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("affiliates/conversions")]
    public async Task<IActionResult> AffiliateConversions()
    {
        var data = await _db.Leads
            .GroupBy(x => x.AffiliateName)
            .Select(g => new
            {
                affiliate = g.Key,
                leads = g.Count(),
                sold = g.Count(x => x.LeadStatus == "Sold"),
                sales = g.Sum(x => x.Sales ?? 0),
                profit = g.Sum(x => x.Profit ?? 0)
            })
            .OrderByDescending(x => x.leads)
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("contracts")]
    public async Task<IActionResult> Contracts()
    {
        var data =
            await (
                from d in _db.LeadDeliveries
                join c in _db.Clients
                    on d.ClientId equals c.Id
                group new { d, c }
                by new
                {
                    c.Id,
                    c.ClientName
                }
                into g
                select new
                {
                    clientId = g.Key.Id,
                    clientName = g.Key.ClientName,
                    deliveredLeads = g.Count(),
                    lastDelivery = g.Max(x => x.d.DeliveredOn)
                }
            )
            .OrderByDescending(x => x.deliveredLeads)
            .ToListAsync();

        return Ok(data);
    }


    [HttpGet("affiliates/daily-report")]
    public async Task<IActionResult> AffiliateDailyReport()
    {
        var data = await _db.Leads
            .GroupBy(x => new
            {
                x.AffiliateName,
                ReportDate = x.CreatedAt.Date
            })
            .Select(g => new
            {
                affiliate = g.Key.AffiliateName,
                reportDate = g.Key.ReportDate,
                leads = g.Count(),
                sold = g.Count(x => x.LeadStatus == "Sold"),
                sales = g.Sum(x => x.Sales ?? 0),
                profit = g.Sum(x => x.Profit ?? 0)
            })
            .OrderByDescending(x => x.reportDate)
            .ToListAsync();

        return Ok(data);
    }

}