using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.LeadRejections;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/lead-rejections")]
    [Authorize]
    public class LeadRejectionsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public LeadRejectionsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetRejections(
            [FromQuery] string? search,
            [FromQuery] string? source,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query =
                from rejection in _db.LeadRejections
                join lead in _db.Leads on rejection.LeadId equals lead.Id
                where lead.IsDeleted == false
                select new
                {
                    rejection,
                    lead
                };

            if (!string.IsNullOrWhiteSpace(source))
            {
                query = query.Where(x => x.rejection.RejectionSource == source);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.lead.Email != null && x.lead.Email.Contains(search)) ||
                    (x.lead.FullName != null && x.lead.FullName.Contains(search)) ||
                    (x.lead.CampaignName != null && x.lead.CampaignName.Contains(search)) ||
                    x.rejection.RejectionReason.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.rejection.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeadRejectionListDto
                {
                    Id = x.rejection.Id,
                    LeadId = x.rejection.LeadId,
                    LeadUuid = x.lead.LeadUuid,
                    Email = x.lead.Email,
                    FullName = x.lead.FullName,
                    CampaignName = x.lead.CampaignName,
                    SourceName = x.lead.AffiliateName,
                    RejectionReason = x.rejection.RejectionReason,
                    RejectionSource = x.rejection.RejectionSource,
                    Notes = x.rejection.Notes,
                    CreatedOn = x.rejection.CreatedOn,
                    CreatedByUserId = x.rejection.CreatedByUserId
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
        public async Task<IActionResult> CreateRejection(CreateLeadRejectionDto model)
        {
            if (string.IsNullOrWhiteSpace(model.RejectionReason))
                return BadRequest(new { message = "Rejection reason is required." });

            var lead = await _db.Leads.FirstOrDefaultAsync(x =>
                x.Id == model.LeadId && x.IsDeleted == false);

            if (lead == null)
                return BadRequest(new { message = "Invalid lead selected." });

            var rejection = new LeadRejection
            {
                LeadId = model.LeadId,
                RejectionReason = model.RejectionReason,
                RejectionSource = string.IsNullOrWhiteSpace(model.RejectionSource)
                    ? "Admin"
                    : model.RejectionSource,
                Notes = model.Notes,
                CreatedByUserId = model.CreatedByUserId,
                CreatedOn = DateTime.UtcNow
            };

            _db.LeadRejections.Add(rejection);

            lead.LeadStatus = "Rejected";
            lead.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Lead rejection created successfully.",
                id = rejection.Id
            });
        }
    }
}