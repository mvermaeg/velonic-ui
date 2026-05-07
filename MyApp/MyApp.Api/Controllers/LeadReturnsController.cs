using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.LeadReturns;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/lead-returns")]
    [Authorize]
    public class LeadReturnsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public LeadReturnsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetReturns(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] long? clientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query =
                from ret in _db.LeadReturns
                join lead in _db.Leads on ret.LeadId equals lead.Id
                join client in _db.Clients on ret.ClientId equals client.Id
                where lead.IsDeleted == false && client.IsDeleted == false
                select new
                {
                    ret,
                    lead,
                    client
                };

            if (clientId.HasValue)
                query = query.Where(x => x.ret.ClientId == clientId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.ret.ReturnStatus == status);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.lead.Email != null && x.lead.Email.Contains(search)) ||
                    (x.lead.FullName != null && x.lead.FullName.Contains(search)) ||
                    (x.lead.CampaignName != null && x.lead.CampaignName.Contains(search)) ||
                    x.client.ClientName.Contains(search) ||
                    x.ret.ReturnReason.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.ret.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeadReturnListDto
                {
                    Id = x.ret.Id,
                    LeadId = x.ret.LeadId,
                    LeadUuid = x.lead.LeadUuid,
                    ClientId = x.ret.ClientId,
                    ClientName = x.client.ClientName,
                    LeadDeliveryId = x.ret.LeadDeliveryId,
                    ReturnReason = x.ret.ReturnReason,
                    ReturnStatus = x.ret.ReturnStatus,
                    Notes = x.ret.Notes,
                    CreatedOn = x.ret.CreatedOn,
                    ReviewedOn = x.ret.ReviewedOn,
                    Email = x.lead.Email,
                    CampaignName = x.lead.CampaignName,
                    SourceName = x.lead.AffiliateName
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateReturn(CreateLeadReturnDto model)
        {
            if (string.IsNullOrWhiteSpace(model.ReturnReason))
                return BadRequest(new { message = "Return reason is required." });

            var leadExists = await _db.Leads.AnyAsync(x =>
                x.Id == model.LeadId && x.IsDeleted == false);

            if (!leadExists)
                return BadRequest(new { message = "Invalid lead selected." });

            var clientExists = await _db.Clients.AnyAsync(x =>
                x.Id == model.ClientId && x.IsDeleted == false);

            if (!clientExists)
                return BadRequest(new { message = "Invalid client selected." });

            var returnRow = new LeadReturn
            {
                LeadId = model.LeadId,
                ClientId = model.ClientId,
                LeadDeliveryId = model.LeadDeliveryId,
                ReturnReason = model.ReturnReason,
                ReturnStatus = "Pending",
                Notes = model.Notes,
                CreatedOn = DateTime.UtcNow
            };

            _db.LeadReturns.Add(returnRow);

            var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == model.LeadId);
            if (lead != null)
            {
                lead.LeadStatus = "Return Requested";
                lead.UpdatedOn = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Lead return created successfully.",
                id = returnRow.Id
            });
        }

        [HttpPut("{id}/review")]
        public async Task<IActionResult> ReviewReturn(
            long id,
            [FromQuery] string status,
            [FromQuery] string? reviewedByUserId)
        {
            if (status != "Approved" && status != "Rejected")
                return BadRequest(new { message = "Status must be Approved or Rejected." });

            var returnRow = await _db.LeadReturns.FirstOrDefaultAsync(x => x.Id == id);

            if (returnRow == null)
                return NotFound(new { message = "Return not found." });

            returnRow.ReturnStatus = status;
            returnRow.ReviewedOn = DateTime.UtcNow;
            returnRow.ReviewedByUserId = reviewedByUserId;

            var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == returnRow.LeadId);
            if (lead != null)
            {
                lead.LeadStatus = status == "Approved" ? "Returned" : "Sold";
                lead.UpdatedOn = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Lead return {status.ToLower()} successfully."
            });
        }
    }
}