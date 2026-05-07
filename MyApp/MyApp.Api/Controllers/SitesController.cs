using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.Sites;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SitesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SitesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetSites()
        {
            var sites = await _db.Sites
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteName,
                    x.DomainName,
                    x.Slug,
                    x.ThemeKey,
                    x.WebsiteTemplateId,
                    x.WebsiteThemeId,
                    TemplateName = x.WebsiteTemplate != null ? x.WebsiteTemplate.TemplateName : null,
                    TemplateKey = x.WebsiteTemplate != null ? x.WebsiteTemplate.TemplateKey : null,
                    ThemeName = x.WebsiteTheme != null ? x.WebsiteTheme.ThemeName : null,
                    ThemeKeyName = x.WebsiteTheme != null ? x.WebsiteTheme.ThemeKey : null,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(sites);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSite(CreateSiteDto model)
        {
            var site = new Site
            {
                SiteName = model.SiteName,
                DomainName = model.DomainName,
                Slug = model.Slug,
                ThemeKey = model.ThemeKey,
                WebsiteTemplateId = model.WebsiteTemplateId,
                WebsiteThemeId = model.WebsiteThemeId,
                IsActive = model.IsActive,
                CreatedOn = DateTime.UtcNow
            };

            _db.Sites.Add(site);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Site created successfully.", id = site.Id });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var site = await _db.Sites.FindAsync(id);

            if (site == null)
                return NotFound(new { message = "Site not found." });

            site.IsActive = isActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site status updated successfully." });
        }

        [HttpPut("{id}/template-theme")]
        public async Task<IActionResult> UpdateTemplateTheme(long id, UpdateSiteTemplateThemeDto model)
        {
            var site = await _db.Sites.FindAsync(id);

            if (site == null)
                return NotFound(new { message = "Site not found." });

            site.WebsiteTemplateId = model.WebsiteTemplateId;
            site.WebsiteThemeId = model.WebsiteThemeId;
            site.ThemeKey = model.ThemeKey;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site template/theme updated successfully." });
        }
    }
}