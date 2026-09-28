using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Routing;
using Microsoft.Extensions.Options;

namespace MyApp.Api.Controllers;

[ApiController]
[Authorize(Roles = "SuperAdmin,Admin")]
[Route("api/lead-routing")]
public sealed class LeadRoutingController : ControllerBase
{
    private readonly MyAppDbContext _db;

    public LeadRoutingController(MyAppDbContext db) => _db = db;

    [HttpPost("runs/{id:long}/reconcile")]
    public async Task<IActionResult> Reconcile(long id, ReconciliationRequest request,
        [FromServices] ExternalAuctionEngine engine, CancellationToken ct)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.EvidenceReference ?? "", @"\A[A-Za-z0-9_-]{3,100}\z"))
            return BadRequest(new { message = "Use a 3–100 character provider support-case reference (letters, digits, underscore or hyphen); no contact data." });
        var actor = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        return await engine.ReconcileAsync(id, request.Accepted, request.EvidenceReference!, actor, ct)
            ? Ok(new { message = "Provider-confirmed outcome recorded. No post or fallback was sent." })
            : Conflict(new { message = "Run is busy, already reconciled or does not require winner reconciliation." });
    }

    [HttpGet("networx-readiness")]
    public async Task<IActionResult> NetworxReadiness(CancellationToken ct, [FromQuery] int? leadTypeId = null)
    {
        var types = await _db.LeadTypes.AsNoTracking().Where(x => x.IsActive).Select(x => new { x.Id, x.Name }).ToListAsync(ct);
        var mappings = await _db.ExternalPlatformTaskMappings.AsNoTracking().Where(x => x.PlatformCode == "NETWORX" && x.IsActive)
            .Select(x => new { x.LeadTypeId, x.InternalOptionCode, x.ExternalTaskId }).ToListAsync(ct);
        var requirements = MyApp.Api.Services.ExternalDeliveries.Networx.NetworxLeadProvider.RequiredTaskOptions;
        var rows = types.Where(t => leadTypeId == null || t.Id == leadTypeId).SelectMany(t => requirements.SelectMany(v => v.Value.Select(option => {
            var matching = mappings.Where(m => t.Id == m.LeadTypeId && m.InternalOptionCode == option).ToList();
            return new { vertical = v.Key, option, leadTypeIds = new[] { t.Id }, leadTypeName = t.Name,
                ready = matching.Count == 1 && !string.IsNullOrWhiteSpace(matching[0].ExternalTaskId),
                reason = matching.Count != 1 || string.IsNullOrWhiteSpace(matching[0].ExternalTaskId) ? "Requires exactly one active nonempty external task ID for this LeadTypeId and option." : null };
        })));
        return Ok(new { items = rows, note = "Use the actual incoming LeadTypeId: website leads may reuse ID 2 across services. Each listed combination is usable only if mapped; map only contracted combinations. Each lead is checked before HTTP. Gutters is unsupported." });
    }

    [HttpGet("auction-capabilities")]
    public IActionResult Capabilities([FromServices] IOptions<ExternalLeadAuctionOptions> options,
        [FromServices] IAuctionGateway gateway) => Ok(new { options = new { options.Value.Enabled,
            options.Value.AuctionWindowSeconds, options.Value.AllowWinnerFallback, options.Value.ThumbtackErrorPolicy,
            options.Value.MaxWinnerPostAttempts, options.Value.PostTimeoutSeconds, options.Value.RetryDelaySeconds },
            providers = AuctionGateway.Capabilities.Select(x => new { capability = x,
                enabledVerticals = new[] { "Roofing", "Windows", "Bathroom", "HVAC", "Gutters" }.Where(v => gateway.Enabled(x.PlatformCode, v)),
                idempotentPost = gateway.Idempotent(x.PlatformCode),
                retryNote = "Unknown post outcomes require reconciliation unless remote idempotency is documented." }) });

    [HttpGet("runs/{id:long}")]
    public async Task<IActionResult> Detail(long id, CancellationToken ct)
    {
        var run = await _db.LeadRoutingRuns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (run == null) return NotFound();
        var attempts = await _db.LeadRoutingAttempts.AsNoTracking().Where(x => x.LeadRoutingRunId == id)
            .OrderBy(x => x.CreatedOn).Select(x => new { x.Id, x.PlatformCode, x.Status, x.IsWinner,
                x.OfferedBidAmount, x.BidReceivedOn, x.BidExpiresOn, x.ResponseDurationMilliseconds,
                hasPostReference = x.PingReferenceId != null, x.PostAttempts, x.PostStartedOn,
                x.CompletedOn, x.HttpStatusCode, x.ErrorMessage, x.PingRequestAudit, x.PingResponseAudit,
                x.PostRequestAudit, x.PostResponseAudit }).ToListAsync(ct);
        return Ok(new { run, attempts });
    }

    [HttpGet("rules")]
    public async Task<IActionResult> GetRules(CancellationToken ct)
    {
        var rows = await _db.LeadRoutingRules.AsNoTracking()
            .OrderBy(x => x.VerticalCode).ThenByDescending(x => x.BidAmount)
            .ThenBy(x => x.Priority).ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPost("rules")]
    public async Task<IActionResult> SaveRule([FromBody] LeadRoutingRule model, CancellationToken ct)
    {
        model.DestinationType = (model.DestinationType ?? "").Trim();
        model.VerticalCode = (model.VerticalCode ?? "").Trim();
        model.PlatformCode = string.IsNullOrWhiteSpace(model.PlatformCode) ? null : model.PlatformCode.Trim();
        if (model.DestinationType == "ExternalPlatform")
        {
            model.PlatformCode = AuctionGateway.Capabilities.FirstOrDefault(x => string.Equals(x.PlatformCode, model.PlatformCode, StringComparison.OrdinalIgnoreCase))?.PlatformCode;
            model.VerticalCode = new[] { "Roofing", "Windows", "Bathroom", "HVAC", "Gutters" }
                .FirstOrDefault(x => string.Equals(x, model.VerticalCode, StringComparison.OrdinalIgnoreCase)) ?? "";
        }
        model.State = string.IsNullOrWhiteSpace(model.State) ? null : model.State.Trim().ToUpperInvariant();
        model.Postcode = string.IsNullOrWhiteSpace(model.Postcode) ? null : model.Postcode.Trim();
        if (model.Postcode != null)
        {
            model.Postcode = MyApp.Api.Services.Thumbtack.ThumbtackCoverageLookup.NormalizeZip(model.Postcode);
            if (model.Postcode == null) return BadRequest(new { message = "ZIP must contain four or five digits." });
        }
        if (model.EffectiveFrom != null && model.EffectiveTo <= model.EffectiveFrom || model.DailyCap is < 0 || model.MonthlyCap is < 0)
            return BadRequest(new { message = "Invalid active dates or caps." });

        if (model.DestinationType is not ("InternalClient" or "ExternalPlatform"))
            return BadRequest(new { message = "DestinationType must be InternalClient or ExternalPlatform." });
        if (string.IsNullOrWhiteSpace(model.VerticalCode) || model.BidAmount < 0)
            return BadRequest(new { message = "Vertical and a non-negative bid are required." });
        if (model.DestinationType == "InternalClient" && model.ClientId == null)
            return BadRequest(new { message = "ClientId is required for an internal client." });
        if (model.DestinationType == "ExternalPlatform" && string.IsNullOrWhiteSpace(model.PlatformCode))
            return BadRequest(new { message = "PlatformCode is required for an external platform." });

        var now = DateTime.UtcNow;
        if (model.Id == 0)
        {
            model.CreatedOn = now;
            model.UpdatedOn = now;
            _db.LeadRoutingRules.Add(model);
        }
        else
        {
            var row = await _db.LeadRoutingRules.FirstOrDefaultAsync(x => x.Id == model.Id, ct);
            if (row == null) return NotFound();
            row.DestinationType = model.DestinationType;
            row.ClientId = model.ClientId;
            row.PlatformCode = model.PlatformCode;
            row.VerticalCode = model.VerticalCode;
            row.State = model.State;
            row.Postcode = model.Postcode;
            row.BidAmount = model.BidAmount;
            row.Priority = model.Priority;
            row.DailyCap = model.DailyCap;
            row.MonthlyCap = model.MonthlyCap;
            row.IsActive = model.IsActive;
            row.EffectiveFrom = model.EffectiveFrom;
            row.EffectiveTo = model.EffectiveTo;
            row.UpdatedOn = now;
        }
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "Routing rule saved safely.", id = model.Id });
    }

    [HttpPut("rules/{id:long}/status")]
    public async Task<IActionResult> SetStatus(long id, [FromQuery] bool isActive, CancellationToken ct)
    {
        var row = await _db.LeadRoutingRules.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return NotFound();
        row.IsActive = isActive;
        row.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = isActive ? "Rule enabled." : "Rule disabled." });
    }

    [HttpGet("runs")]
    public async Task<IActionResult> GetRuns([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.LeadRoutingRuns.AsNoTracking().OrderByDescending(x => x.CreatedOn);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return Ok(new { total, page, pageSize, items });
    }
}
public sealed record ReconciliationRequest(bool Accepted, string EvidenceReference);
