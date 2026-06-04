using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitePageSectionsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SitePageSectionsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.SitePageSections
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("by-page/{pageId}")]
        public async Task<IActionResult> GetByPage(long pageId)
        {
            var data = await _db.SitePageSections
                .Where(x => x.SitePageId == pageId)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SitePageSection model)
        {
            model.CreatedOn = DateTime.UtcNow;

            _db.SitePageSections.Add(model);

            await _db.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, SitePageSection model)
        {
            var entity = await _db.SitePageSections
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(new { message = "Section not found." });

            entity.SectionKey = model.SectionKey;
            entity.SectionTitle = model.SectionTitle;
            entity.SectionSubtitle = model.SectionSubtitle;
            entity.HtmlContent = model.HtmlContent;
            entity.ImageUrl = model.ImageUrl;
            entity.ButtonText = model.ButtonText;
            entity.ButtonUrl = model.ButtonUrl;
            entity.SortOrder = model.SortOrder;
            entity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(entity);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, bool isActive)
        {
            var entity = await _db.SitePageSections
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(new { message = "Section not found." });

            entity.IsActive = isActive;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true
            });
        }
    }
}


//using Microsoft.AspNetCore.Authorization;
 //using Microsoft.AspNetCore.Mvc;
 //using Microsoft.EntityFrameworkCore;
 //using MyApp.Api.Data;
 //using MyApp.Api.Data.Entities;

//namespace MyApp.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class SitePageSectionsController : ControllerBase
//    {
//        private readonly MyAppDbContext _db;

//        public SitePageSectionsController(MyAppDbContext db)
//        {
//            _db = db;
//        }

//        [HttpGet("by-page/{sitePageId}")]
//        public async Task<IActionResult> GetByPage(long sitePageId)
//        {
//            var data = await _db.SitePageSections
//                .Where(x => x.SitePageId == sitePageId)
//                .OrderBy(x => x.SortOrder)
//                .Select(x => new
//                {
//                    x.Id,
//                    x.SitePageId,
//                    x.SectionKey,
//                    x.SectionTitle,
//                    x.SectionSubtitle,
//                    x.HtmlContent,
//                    x.ImageUrl,
//                    x.ButtonText,
//                    x.ButtonUrl,
//                    x.SortOrder,
//                    x.IsActive,
//                    x.CreatedOn
//                })
//                .ToListAsync();

//            return Ok(data);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(SaveSitePageSectionDto model)
//        {
//            var pageExists = await _db.SitePages.AnyAsync(x => x.Id == model.SitePageId);

//            if (!pageExists)
//                return BadRequest(new { message = "Invalid page selected." });

//            var section = new SitePageSection
//            {
//                SitePageId = model.SitePageId,
//                SectionKey = model.SectionKey,
//                SectionTitle = model.SectionTitle,
//                SectionSubtitle = model.SectionSubtitle,
//                HtmlContent = model.HtmlContent,
//                ImageUrl = model.ImageUrl,
//                ButtonText = model.ButtonText,
//                ButtonUrl = model.ButtonUrl,
//                SortOrder = model.SortOrder,
//                IsActive = model.IsActive,
//                CreatedOn = DateTime.UtcNow
//            };

//            _db.SitePageSections.Add(section);
//            await _db.SaveChangesAsync();

//            return Ok(new { message = "Section created successfully.", id = section.Id });
//        }

//        [HttpPut("{id}")]
//        public async Task<IActionResult> Update(long id, SaveSitePageSectionDto model)
//        {
//            var section = await _db.SitePageSections.FindAsync(id);

//            if (section == null)
//                return NotFound(new { message = "Section not found." });

//            section.SitePageId = model.SitePageId;
//            section.SectionKey = model.SectionKey;
//            section.SectionTitle = model.SectionTitle;
//            section.SectionSubtitle = model.SectionSubtitle;
//            section.HtmlContent = model.HtmlContent;
//            section.ImageUrl = model.ImageUrl;
//            section.ButtonText = model.ButtonText;
//            section.ButtonUrl = model.ButtonUrl;
//            section.SortOrder = model.SortOrder;
//            section.IsActive = model.IsActive;

//            await _db.SaveChangesAsync();

//            return Ok(new { message = "Section updated successfully." });
//        }

//        [HttpPut("{id}/status")]
//        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
//        {
//            var section = await _db.SitePageSections.FindAsync(id);

//            if (section == null)
//                return NotFound(new { message = "Section not found." });

//            section.IsActive = isActive;

//            await _db.SaveChangesAsync();

//            return Ok(new { message = "Section status updated successfully." });
//        }
//    }

//    public class SaveSitePageSectionDto
//    {
//        public long SitePageId { get; set; }
//        public string SectionKey { get; set; } = string.Empty;
//        public string? SectionTitle { get; set; }
//        public string? SectionSubtitle { get; set; }
//        public string? HtmlContent { get; set; }
//        public string? ImageUrl { get; set; }
//        public string? ButtonText { get; set; }
//        public string? ButtonUrl { get; set; }
//        public int SortOrder { get; set; }
//        public bool IsActive { get; set; } = true;
//    }
//}