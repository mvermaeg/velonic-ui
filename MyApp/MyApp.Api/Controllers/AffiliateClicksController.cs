using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.AffiliateClicks;

namespace MyApp.Api.Controllers
{
    [ApiController]
    public class AffiliateClicksController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public AffiliateClicksController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpPost("api/public/affiliate-clicks/track")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackClick(TrackAffiliateClickDto model)
        {
            Affiliate? affiliate = null;

            if (!string.IsNullOrWhiteSpace(model.ApiKey))
            {
                affiliate = await _db.Affiliates
                    .FirstOrDefaultAsync(x => x.ApiKey == model.ApiKey && x.IsActive == true);
            }

            var click = new AffiliateClick
            {
                AffiliateId = affiliate?.Id,
                AffiliateName = affiliate?.AffiliateName,
                ApiKey = model.ApiKey,
                SubId = model.SubId,
                PageName = model.PageName,
                CampaignName = model.CampaignName,
                SourceName = model.SourceName,
                Url = model.Url,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedOn = DateTime.UtcNow
            };

            _db.AffiliateClicks.Add(click);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Click tracked successfully.",
                clickId = click.Id,
                clickUuid = click.ClickUuid
            });
        }

        [HttpGet("api/affiliate-clicks")]
        [Authorize]
        public async Task<IActionResult> GetClicks(
            [FromQuery] string? search,
            [FromQuery] long? affiliateId,
            [FromQuery] string? campaignName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.AffiliateClicks.AsQueryable();

            if (affiliateId.HasValue)
                query = query.Where(x => x.AffiliateId == affiliateId.Value);

            if (!string.IsNullOrWhiteSpace(campaignName))
                query = query.Where(x => x.CampaignName == campaignName);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.AffiliateName != null && x.AffiliateName.Contains(search)) ||
                    (x.SubId != null && x.SubId.Contains(search)) ||
                    (x.PageName != null && x.PageName.Contains(search)) ||
                    (x.CampaignName != null && x.CampaignName.Contains(search)) ||
                    (x.SourceName != null && x.SourceName.Contains(search)) ||
                    (x.Url != null && x.Url.Contains(search)));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.ClickUuid,
                    x.AffiliateId,
                    x.AffiliateName,
                    x.SubId,
                    x.PageName,
                    x.CampaignName,
                    x.SourceName,
                    x.Url,
                    x.IpAddress,
                    x.UserAgent,
                    x.CreatedOn
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                data
            });
        }
    }
}