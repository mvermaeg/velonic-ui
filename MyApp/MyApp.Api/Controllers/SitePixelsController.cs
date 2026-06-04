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
    public class SitePixelsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SitePixelsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.SitePixels
                .Include(x => x.Site)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    SiteName = x.Site.SiteName,
                    x.PixelType,
                    x.PixelName,
                    x.PixelCode,
                    x.Placement,
                    x.CampaignName,
                    x.SourceName,
                    x.FireOnPageSlug,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("by-site/{siteId}")]
        public async Task<IActionResult> GetBySite(long siteId)
        {
            var data = await _db.SitePixels
                .Where(x => x.SiteId == siteId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SitePixel model)
        {
            model.CreatedOn = DateTime.UtcNow;
            _db.SitePixels.Add(model);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Pixel created successfully.", id = model.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, SitePixel model)
        {
            var entity = await _db.SitePixels.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = "Pixel not found." });

            entity.SiteId = model.SiteId;
            entity.PixelType = model.PixelType;
            entity.PixelName = model.PixelName;
            entity.PixelCode = model.PixelCode;
            entity.Placement = model.Placement;
            entity.CampaignName = model.CampaignName;
            entity.SourceName = model.SourceName;
            entity.FireOnPageSlug = model.FireOnPageSlug;
            entity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Pixel updated successfully." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var entity = await _db.SitePixels.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = "Pixel not found." });

            entity.IsActive = isActive;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Pixel status updated successfully." });
        }
    }
}