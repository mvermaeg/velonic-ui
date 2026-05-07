using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/sites")]
    public class PublicSitesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public PublicSitesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet("by-slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var site = await _db.Sites
                .Where(x => x.Slug == slug && x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.SiteName,
                    x.DomainName,
                    x.Slug,
                    x.ThemeKey,

                    Template = x.WebsiteTemplate == null ? null : new
                    {
                        x.WebsiteTemplate.Id,
                        x.WebsiteTemplate.TemplateName,
                        x.WebsiteTemplate.TemplateKey
                    },

                    Theme = x.WebsiteTheme == null ? null : new
                    {
                        x.WebsiteTheme.Id,
                        x.WebsiteTheme.ThemeName,
                        x.WebsiteTheme.ThemeKey,
                        x.WebsiteTheme.PrimaryColor,
                        x.WebsiteTheme.SecondaryColor,
                        x.WebsiteTheme.FontFamily,
                        x.WebsiteTheme.LogoUrl,
                        x.WebsiteTheme.CustomCss
                    }
                })
                .FirstOrDefaultAsync();

            if (site == null)
                return NotFound(new { message = "Site not found." });

            return Ok(site);
        }
    }
}