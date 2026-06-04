using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SiteFormsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SiteFormsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.SiteForms
                .Include(x => x.Site)
                .Include(x => x.SitePage)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    SiteName = x.Site.SiteName,
                    x.SitePageId,
                    PageName = x.SitePage == null ? null : x.SitePage.PageName,
                    x.FormKey,
                    x.DisplayName,
                    x.SubmitButtonText,
                    x.SuccessMessage,
                    x.CampaignName,
                    x.SourceName,
                    x.IsMultiStep,
                    x.SettingsJson,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("by-site/{siteId}")]
        public async Task<IActionResult> GetBySite(long siteId)
        {
            var data = await _db.SiteForms
                .Where(x => x.SiteId == siteId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SiteForm model)
        {
            model.CreatedOn = DateTime.UtcNow;

            _db.SiteForms.Add(model);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Site form created successfully.", id = model.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, SiteForm model)
        {
            var entity = await _db.SiteForms.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = "Site form not found." });

            entity.SiteId = model.SiteId;
            entity.SitePageId = model.SitePageId;
            entity.FormKey = model.FormKey;
            entity.DisplayName = model.DisplayName;
            entity.SubmitButtonText = model.SubmitButtonText;
            entity.SuccessMessage = model.SuccessMessage;
            entity.CampaignName = model.CampaignName;
            entity.SourceName = model.SourceName;
            entity.IsMultiStep = model.IsMultiStep;
            entity.SettingsJson = model.SettingsJson;
            entity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site form updated successfully." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var entity = await _db.SiteForms.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = "Site form not found." });

            entity.IsActive = isActive;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Site form status updated successfully." });
        }
    }
}