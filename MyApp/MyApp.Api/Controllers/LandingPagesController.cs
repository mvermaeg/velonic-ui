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
    public class LandingPagesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public LandingPagesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetLandingPages([FromQuery] long? siteId)
        {
            var query = _db.LandingPages.AsQueryable();

            if (siteId.HasValue)
                query = query.Where(x => x.SiteId == siteId.Value);

            var pages = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(pages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLandingPage(LandingPage model)
        {
            model.CreatedOn = DateTime.UtcNow;
            _db.LandingPages.Add(model);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Landing page created successfully.", id = model.Id });
        }
    }
}