using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Data;
using MyApp.Api.Services.Reporting;

namespace MyApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("api/reports/dashboard")]
public sealed class DashboardController(MyAppDbContext db) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<DashboardSummary>> Get(CancellationToken ct) =>
        Ok(await DashboardReport.ReadAsync(db, DateTime.UtcNow, ct));
}
