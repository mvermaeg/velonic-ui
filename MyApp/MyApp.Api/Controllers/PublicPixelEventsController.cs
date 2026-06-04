using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/pixel-events")]
    public class PublicPixelEventsController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public PublicPixelEventsController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpPost("track")]
        public async Task<IActionResult> Track(TrackPixelEventDto model)
        {
            if (model.SiteId <= 0)
                return BadRequest(new { message = "Invalid site." });

            var entity = new SitePixelEvent
            {
                SiteId = model.SiteId,
                SitePixelId = model.SitePixelId,
                PageSlug = model.PageSlug,
                EventName = model.EventName,
                Url = model.Url,
                UserAgent = Request.Headers.UserAgent.ToString(),
                Referrer = Request.Headers.Referer.ToString(),
                CreatedOn = DateTime.UtcNow
            };

            _db.SitePixelEvents.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Pixel event tracked.", id = entity.Id });
        }
    }

    public class TrackPixelEventDto
    {
        public long SiteId { get; set; }
        public long? SitePixelId { get; set; }
        public string? PageSlug { get; set; }
        public string EventName { get; set; } = "PageView";
        public string? Url { get; set; }
    }
}