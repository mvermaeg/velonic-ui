using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.DTOs.Leads;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public LeadsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeads(
            [FromQuery] string? search,
            [FromQuery] string? campaignName,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.Leads
                .Where(x => x.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)) ||
                    (x.Phone != null && x.Phone.Contains(search)) ||
                    (x.Postcode != null && x.Postcode.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(campaignName))
            {
                query = query.Where(x => x.CampaignName == campaignName);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.LeadStatus == status);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeadListDto
                {
                    Id = x.Id,
                    LeadUuid = x.LeadUuid,
                    CreatedAt = x.CreatedAt,
                    ReceivedAt = x.ReceivedAt,

                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,

                    CampaignName = x.CampaignName,
                    AffiliateName = x.AffiliateName,
                    PageName = x.PageName,

                    Postcode = x.Postcode,
                    State = x.State,
                    City = x.City,

                    Step = x.Step,
                    IsCompleted = x.IsCompleted,
                    LeadStatus = x.LeadStatus
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
    }
}