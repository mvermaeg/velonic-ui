using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteDomainsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SiteDomainsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _db.SiteDomains
                .Include(x => x.Site)
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SiteId,
                    SiteName = x.Site.SiteName,
                    x.DomainName,
                    x.IsPrimary,
                    x.SslEnabled,
                    x.CloudflareEnabled,
                    x.IsActive,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SiteDomain model)
        {
            var exists = await _db.SiteDomains
                .AnyAsync(x => x.DomainName == model.DomainName);

            if (exists)
                return BadRequest(new
                {
                    message = "Domain already exists."
                });

            model.CreatedOn = DateTime.UtcNow;

            _db.SiteDomains.Add(model);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Domain added successfully."
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, SiteDomain model)
        {
            var entity = await _db.SiteDomains
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(new
                {
                    message = "Domain not found."
                });

            entity.DomainName = model.DomainName;
            entity.IsPrimary = model.IsPrimary;
            entity.SslEnabled = model.SslEnabled;
            entity.CloudflareEnabled = model.CloudflareEnabled;
            entity.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Domain updated successfully."
            });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, bool isActive)
        {
            var entity = await _db.SiteDomains
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return NotFound(new
                {
                    message = "Domain not found."
                });

            entity.IsActive = isActive;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true
            });
        }
    }
}