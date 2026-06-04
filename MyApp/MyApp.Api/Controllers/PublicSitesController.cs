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
                    },

                    Settings = x.SiteSettings
                        .Where(s => s.IsActive)
                        .OrderByDescending(s => s.Id)
                        .Select(s => new
                        {
                            s.Id,
                            s.LogoUrl,
                            s.PhoneNumber,
                            s.EmailAddress,
                            s.AddressLine1,
                            s.AddressLine2,
                            s.BusinessHours,
                            s.FacebookUrl,
                            s.LinkedinUrl,
                            s.InstagramUrl,
                            s.TwitterUrl,
                            s.YoutubeUrl
                        })
                        .FirstOrDefault(),

                    Pixels = x.SitePixels
                        .Where(p => p.IsActive)
                        .OrderByDescending(p => p.Id)
                        .Select(p => new
                        {
                            p.Id,
                            p.PixelType,
                            p.PixelName,
                            p.PixelCode,
                            p.Placement,
                            p.CampaignName,
                            p.SourceName,
                            p.FireOnPageSlug
                        })
                        .ToList(),


                    Forms = x.SiteForms
    .Where(f => f.IsActive)
    .OrderByDescending(f => f.Id)
    .Select(f => new
    {
        f.Id,
        f.SiteId,
        f.SitePageId,
        f.FormKey,
        f.DisplayName,
        f.SubmitButtonText,
        f.SuccessMessage,
        f.CampaignName,
        f.SourceName,
        f.IsMultiStep,
        f.SettingsJson
    })
    .ToList(),


                    Pages = x.SitePages
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.SortOrder)
                        .Select(p => new
                        {
                            p.Id,
                            p.PageName,
                            p.PageSlug,
                            p.PageTitle,
                            p.MetaTitle,
                            p.MetaDescription,
                            p.HeroTitle,
                            p.HeroSubtitle,
                            p.HtmlContent,
                            p.JsonContent,
                            p.IsHomePage,
                            p.SortOrder,

                            Sections = p.SitePageSections
                                .Where(s => s.IsActive)
                                .OrderBy(s => s.SortOrder)
                                .Select(s => new
                                {
                                    s.Id,
                                    s.SectionKey,
                                    s.SectionTitle,
                                    s.SectionSubtitle,
                                    s.HtmlContent,
                                    s.ImageUrl,
                                    s.ButtonText,
                                    s.ButtonUrl,
                                    s.SortOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (site == null)
                return NotFound(new { message = "Site not found." });

            return Ok(site);
        }

        [HttpGet("by-domain")]
        public async Task<IActionResult> GetByDomain([FromQuery] string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
                return BadRequest(new { message = "Domain is required." });

            domain = domain.Trim().ToLower();

            var site = await _db.SiteDomains
                .Where(d => d.DomainName.ToLower() == domain && d.IsActive)
                .Select(d => d.Site)
                .Where(x => x.IsActive)
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
                    },

                    Settings = x.SiteSettings
                        .Where(s => s.IsActive)
                        .OrderByDescending(s => s.Id)
                        .Select(s => new
                        {
                            s.Id,
                            s.LogoUrl,
                            s.PhoneNumber,
                            s.EmailAddress,
                            s.AddressLine1,
                            s.AddressLine2,
                            s.BusinessHours,
                            s.FacebookUrl,
                            s.LinkedinUrl,
                            s.InstagramUrl,
                            s.TwitterUrl,
                            s.YoutubeUrl
                        })
                        .FirstOrDefault(),

                    Pixels = x.SitePixels
                        .Where(p => p.IsActive)
                        .OrderByDescending(p => p.Id)
                        .Select(p => new
                        {
                            p.Id,
                            p.PixelType,
                            p.PixelName,
                            p.PixelCode,
                            p.Placement,
                            p.CampaignName,
                            p.SourceName,
                            p.FireOnPageSlug
                        })
                        .ToList(),

                    Forms = x.SiteForms
    .Where(f => f.IsActive)
    .OrderByDescending(f => f.Id)
    .Select(f => new
    {
        f.Id,
        f.SiteId,
        f.SitePageId,
        f.FormKey,
        f.DisplayName,
        f.SubmitButtonText,
        f.SuccessMessage,
        f.CampaignName,
        f.SourceName,
        f.IsMultiStep,
        f.SettingsJson
    })
    .ToList(),

                    Pages = x.SitePages
                        .Where(p => p.IsActive)
                        .OrderBy(p => p.SortOrder)
                        .Select(p => new
                        {
                            p.Id,
                            p.PageName,
                            p.PageSlug,
                            p.PageTitle,
                            p.MetaTitle,
                            p.MetaDescription,
                            p.HeroTitle,
                            p.HeroSubtitle,
                            p.HtmlContent,
                            p.JsonContent,
                            p.IsHomePage,
                            p.SortOrder,

                            Sections = p.SitePageSections
                                .Where(s => s.IsActive)
                                .OrderBy(s => s.SortOrder)
                                .Select(s => new
                                {
                                    s.Id,
                                    s.SectionKey,
                                    s.SectionTitle,
                                    s.SectionSubtitle,
                                    s.HtmlContent,
                                    s.ImageUrl,
                                    s.ButtonText,
                                    s.ButtonUrl,
                                    s.SortOrder
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (site == null)
                return NotFound(new { message = "Site not found." });

            return Ok(site);
        }
    }
}
 