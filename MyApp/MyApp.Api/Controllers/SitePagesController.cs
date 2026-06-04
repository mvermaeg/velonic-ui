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
    public class SitePagesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SitePagesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.SitePages
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    SiteName = x.Site != null ? x.Site.SiteName : null,
                    x.PageName,
                    x.PageSlug,
                    x.PageTitle,
                    x.MetaTitle,
                    x.MetaDescription,
                    x.HeroTitle,
                    x.HeroSubtitle,
                    x.HtmlContent,
                    x.JsonContent,
                    x.IsHomePage,
                    x.SortOrder,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("by-site/{siteId}")]
        public async Task<IActionResult> GetBySite(long siteId)
        {
            var data = await _db.SitePages
                .Where(x => x.SiteId == siteId)
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    x.PageName,
                    x.PageSlug,
                    x.PageTitle,
                    x.MetaTitle,
                    x.MetaDescription,
                    x.HeroTitle,
                    x.HeroSubtitle,
                    x.HtmlContent,
                    x.JsonContent,
                    x.IsHomePage,
                    x.SortOrder,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSitePageDto model)
        {
            var siteExists = await _db.Sites.AnyAsync(x => x.Id == model.SiteId);

            if (!siteExists)
                return BadRequest(new { message = "Invalid site selected." });

            if (model.IsHomePage)
            {
                var oldHomePages = await _db.SitePages
                    .Where(x => x.SiteId == model.SiteId && x.IsHomePage)
                    .ToListAsync();

                foreach (var page in oldHomePages)
                    page.IsHomePage = false;
            }

            var pageEntity = new SitePage
            {
                SiteId = model.SiteId,
                PageName = model.PageName,
                PageSlug = model.PageSlug,
                PageTitle = model.PageTitle,
                MetaTitle = model.MetaTitle,
                MetaDescription = model.MetaDescription,
                HeroTitle = model.HeroTitle,
                HeroSubtitle = model.HeroSubtitle,
                HtmlContent = model.HtmlContent,
                JsonContent = model.JsonContent,
                IsHomePage = model.IsHomePage,
                SortOrder = model.SortOrder,
                IsActive = model.IsActive,
                CreatedOn = DateTime.UtcNow
            };

            _db.SitePages.Add(pageEntity);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Site page created successfully.", id = pageEntity.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CreateSitePageDto model)
        {
            var pageEntity = await _db.SitePages.FindAsync(id);

            if (pageEntity == null)
                return NotFound(new { message = "Site page not found." });

            var siteExists = await _db.Sites.AnyAsync(x => x.Id == model.SiteId);

            if (!siteExists)
                return BadRequest(new { message = "Invalid site selected." });

            if (model.IsHomePage)
            {
                var oldHomePages = await _db.SitePages
                    .Where(x => x.SiteId == model.SiteId && x.Id != id && x.IsHomePage)
                    .ToListAsync();

                foreach (var page in oldHomePages)
                    page.IsHomePage = false;
            }

            pageEntity.SiteId = model.SiteId;
            pageEntity.PageName = model.PageName;
            pageEntity.PageSlug = model.PageSlug;
            pageEntity.PageTitle = model.PageTitle;
            pageEntity.MetaTitle = model.MetaTitle;
            pageEntity.MetaDescription = model.MetaDescription;
            pageEntity.HeroTitle = model.HeroTitle;
            pageEntity.HeroSubtitle = model.HeroSubtitle;
            pageEntity.HtmlContent = model.HtmlContent;
            pageEntity.JsonContent = model.JsonContent;
            pageEntity.IsHomePage = model.IsHomePage;
            pageEntity.SortOrder = model.SortOrder;
            pageEntity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site page updated successfully." });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var pageEntity = await _db.SitePages.FindAsync(id);

            if (pageEntity == null)
                return NotFound(new { message = "Site page not found." });

            pageEntity.IsActive = isActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site page status updated successfully." });
        }
       

    }

    public class CreateSitePageDto
    {
        public long SiteId { get; set; }

        public string PageName { get; set; } = string.Empty;

        public string PageSlug { get; set; } = string.Empty;

        public string? PageTitle { get; set; }

        public string? MetaTitle { get; set; }

        public string? MetaDescription { get; set; }

        public string? HeroTitle { get; set; }

        public string? HeroSubtitle { get; set; }

        public string? HtmlContent { get; set; }

        public string? JsonContent { get; set; }

        public bool IsHomePage { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}