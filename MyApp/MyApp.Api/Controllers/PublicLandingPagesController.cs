using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/pages")]
    public class PublicLandingPagesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public PublicLandingPagesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{siteSlug}/{pageSlug}")]
        public async Task<IActionResult> GetPublicPage(string siteSlug, string pageSlug)
        {
            var page = await _db.LandingPages
                .Where(x => x.IsActive == true)
                .Join(
                    _db.Sites.Where(s => s.IsActive == true),
                    p => p.SiteId,
                    s => s.Id,
                    (p, s) => new { Page = p, Site = s }
                )
                .Where(x => x.Site.Slug == siteSlug && x.Page.PageSlug == pageSlug)
                .Select(x => new
                {
                    siteId = x.Site.Id,
                    siteName = x.Site.SiteName,
                    siteSlug = x.Site.Slug,
                    domainName = x.Site.DomainName,
                    themeKey = x.Site.ThemeKey,

                    landingPageId = x.Page.Id,
                    pageName = x.Page.PageName,
                    pageSlug = x.Page.PageSlug,
                    campaignName = x.Page.CampaignName,
                    affiliateName = x.Page.AffiliateName,

                    pageTitle = x.Page.PageTitle,
                    heroTitle = x.Page.HeroTitle,
                    heroSubtitle = x.Page.HeroSubtitle,
                    ctaText = x.Page.CtaText
                })
                .FirstOrDefaultAsync();

            if (page == null)
                return NotFound(new { message = "Page not found." });

            return Ok(page);
        }
    }
}