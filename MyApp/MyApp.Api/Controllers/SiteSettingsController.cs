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
    public class SiteSettingsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public SiteSettingsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet("by-site/{siteId}")]
        public async Task<IActionResult> GetBySite(long siteId)
        {
            var data = await _db.SiteSettings
                .Where(x => x.SiteId == siteId)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            return Ok(data);
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(SaveSiteSettingDto model)
        {
            var setting = await _db.SiteSettings
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.SiteId == model.SiteId);

            if (setting == null)
            {
                setting = new SiteSetting
                {
                    SiteId = model.SiteId,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                _db.SiteSettings.Add(setting);
            }

            setting.LogoUrl = model.LogoUrl;
            setting.PhoneNumber = model.PhoneNumber;
            setting.EmailAddress = model.EmailAddress;
            setting.AddressLine1 = model.AddressLine1;
            setting.AddressLine2 = model.AddressLine2;
            setting.BusinessHours = model.BusinessHours;
            setting.FacebookUrl = model.FacebookUrl;
            setting.LinkedinUrl = model.LinkedinUrl;
            setting.InstagramUrl = model.InstagramUrl;
            setting.TwitterUrl = model.TwitterUrl;
            setting.YoutubeUrl = model.YoutubeUrl;
            setting.IsActive = model.IsActive;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Site settings saved successfully.", id = setting.Id });
        }
    }

    public class SaveSiteSettingDto
    {
        public long SiteId { get; set; }
        public string? LogoUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? BusinessHours { get; set; }
        public string? FacebookUrl { get; set; }
        public string? LinkedinUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? YoutubeUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}