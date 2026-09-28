using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Thumbtack;

namespace MyApp.Api.Controllers;

[ApiController, Authorize(Roles = "SuperAdmin,Admin"), Route("api/lead-routing/coverage")]
public sealed class ThumbtackCoverageController(MyAppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? service, [FromQuery] string? zip, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var query = db.ThumbtackZipCoverages.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(service)) query = query.Where(x => x.ServiceCode == service);
        if (!string.IsNullOrWhiteSpace(zip))
        {
            zip = ThumbtackCoverageLookup.NormalizeZip(zip);
            if (zip == null) return BadRequest(new { message = "ZIP must contain four or five digits." });
            query = query.Where(x => x.ZipCode == zip);
        }
        page = Math.Clamp(page, 1, 1_000_000);
        return Ok(new { services = await db.ThumbtackCoverageServices.AsNoTracking().ToListAsync(ct),
            total = await query.CountAsync(ct), page = Math.Max(1, page),
            items = await query.OrderBy(x => x.ServiceCode).ThenBy(x => x.ZipCode).Skip((Math.Max(1, page) - 1) * 50).Take(50).ToListAsync(ct),
            latest = await db.ThumbtackCoverageImportBatches.AsNoTracking().OrderByDescending(x => x.Id)
                .Select(x => new { x.Id, x.SourceName, x.ImportedOn, x.SourceRowCount, x.NormalizedZipCount, x.PaddedRowCount, x.InsertedPairCount }).FirstOrDefaultAsync(ct) });
    }
    [HttpPut("status")]
    public async Task<IActionResult> Status(CoverageStatus request, CancellationToken ct)
    {
        if (request.Zip == null)
        {
            var service = await db.ThumbtackCoverageServices.FindAsync([request.Service], ct);
            if (service == null) return NotFound(); service.IsEnabled = request.Enabled;
        }
        else
        {
            var zip = ThumbtackCoverageLookup.NormalizeZip(request.Zip);
            if (zip == null) return BadRequest(new { message = "ZIP must contain four or five digits." });
            var row = await db.ThumbtackZipCoverages.FindAsync([request.Service, zip!], ct);
            if (row == null) return NotFound(); row.IsEnabled = request.Enabled;
        }
        await db.SaveChangesAsync(ct); return Ok(new { message = "Coverage status updated." });
    }
    [HttpPost("import"), RequestSizeLimit(2_000_000)]
    public async Task<IActionResult> Import(CoverageImport request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 1_500_000)
            return BadRequest(new { message = "Provide a nonempty coverage file of at most 1.5 million characters." });
        var services = await db.ThumbtackCoverageServices.AsNoTracking().ToListAsync(ct);
        var parsed = CoverageParser.Parse(request.Text, request.Service, services.Select(x => x.ServiceCode));
        if (!request.Confirm || parsed.Errors.Count > 0)
            return Ok(new { applied = false, rows = parsed.Rows.Count, errors = parsed.Errors,
                message = parsed.Errors.Count > 0 ? "No rows imported or excluded. Resolve every reported row first." : "Preview passed; confirm to apply." });
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { parsed.Rows, request.Replace }))));
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource='Homeyy.ThumbtackCoverageImport', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=30000; IF @r<0 THROW 51100, 'Coverage import busy.', 1;", ct);
        var existing = await db.ThumbtackCoverageImportBatches.AnyAsync(x => x.Fingerprint == fingerprint, ct);
        if (existing) { await tx.RollbackAsync(ct); return Ok(new { applied = true, inserted = 0, errors = Array.Empty<string>(), message = "This import was already applied." }); }
        var batch = new ThumbtackCoverageImportBatch { Fingerprint = fingerprint, SourceName = "Admin coverage upload",
            ImportedOn = DateTime.UtcNow, SourceRowCount = parsed.Rows.Count, NormalizedZipCount = parsed.Rows.Select(x => x.Zip).Distinct().Count(),
            PaddedRowCount = parsed.Rows.Count(x => x.Padded), ServicesJson = JsonSerializer.Serialize(parsed.Rows.Select(x => x.Service).Distinct()),
            SourceRowsJson = JsonSerializer.Serialize(parsed.Rows) };
        db.ThumbtackCoverageImportBatches.Add(batch); await db.SaveChangesAsync(ct);
        foreach (var group in parsed.Rows.GroupBy(x => x.Service))
        {
            var rows = await db.ThumbtackZipCoverages.Where(x => x.ServiceCode == group.Key).ToListAsync(ct);
            if (request.Replace) foreach (var row in rows) row.IsEnabled = false;
            foreach (var item in group.DistinctBy(x => x.Zip))
            {
                var row = rows.FirstOrDefault(x => x.ZipCode == item.Zip);
                if (row == null) { row = new() { ServiceCode = item.Service, ZipCode = item.Zip, ImportBatchId = batch.Id }; db.ThumbtackZipCoverages.Add(row); batch.InsertedPairCount++; }
                row.IsEnabled = true;
            }
        }
        await db.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        return Ok(new { applied = true, inserted = batch.InsertedPairCount, errors = Array.Empty<string>(), message = "Coverage imported." });
    }
}
public sealed record CoverageStatus(string Service, string? Zip, bool Enabled);
public sealed record CoverageImport(string Text, string? Service, bool Replace, bool Confirm);
public sealed record CoverageRow(string Service, string Zip, bool Padded);
public sealed record CoverageIssue(int Line, string Raw, string Reason);
public static class CoverageParser
{
    public static (List<CoverageRow> Rows, List<CoverageIssue> Errors) Parse(string text, string? defaultService, IEnumerable<string> knownServices)
    {
        var known = knownServices.ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);
        var rows = new List<CoverageRow>(); var errors = new List<CoverageIssue>();
        using var reader = new StringReader(text ?? "");
        string? line; var index = 0;
        while ((line = reader.ReadLine()) != null)
        {
            index++; if (string.IsNullOrWhiteSpace(line)) continue;
            if (index == 1 && (line.Trim().Equals("Zip Code", StringComparison.OrdinalIgnoreCase) ||
                line.Replace(" ", "").Equals("service,zip", StringComparison.OrdinalIgnoreCase))) continue;
            var parts = line.Split(',');
            var service = parts.Length == 2 ? parts[0].Trim() : defaultService;
            var raw = parts[^1].Trim(); var zip = ThumbtackCoverageLookup.NormalizeZip(raw);
            if (parts.Length > 2 || zip == null || service == null || !known.TryGetValue(service, out var canonical))
            { errors.Add(new(index, line, "Invalid ZIP or unmapped service. Expected ZIP-only with a chosen service, or Service,Zip.")); continue; }
            rows.Add(new(canonical, zip, raw.Length == 4));
        }
        if (rows.Count == 0 && errors.Count == 0) errors.Add(new(0, "", "File contains no coverage rows."));
        return (rows, errors);
    }
}
