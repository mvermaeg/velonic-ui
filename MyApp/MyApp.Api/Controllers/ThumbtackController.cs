using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Services.Thumbtack;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/thumbtack")]
    public class ThumbtackController : ControllerBase
    {
        private readonly ThumbtackService _service;
        private readonly MyAppDbContext _db;

        public ThumbtackController(
            ThumbtackService service,
            MyAppDbContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpPost("session/{leadId:long}")]
        public async Task<IActionResult>
            CreateSession(
                long leadId,
                CancellationToken cancellationToken)
        {
            try
            {
                var result =
                    await _service.CreateSessionAsync(
                        leadId,
                        cancellationToken);

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("session/{sessionId:long}/opened")]
        public async Task<IActionResult>
            MarkOpened(
                long sessionId,
                CancellationToken cancellationToken)
        {
            var session =
                await _db.ThumbtackSessions
                    .FirstOrDefaultAsync(
                        x => x.Id == sessionId,
                        cancellationToken);

            if (session == null)
                return NotFound();

            session.Status =
                "WidgetOpened";

            session.StartedOn ??=
                DateTime.UtcNow;

            session.UpdatedOn =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return Ok();
        }

        [HttpPost("session/{sessionId:long}/pending")]
        public async Task<IActionResult>
            MarkPending(
                long sessionId,
                CancellationToken cancellationToken)
        {
            var session =
                await _db.ThumbtackSessions
                    .FirstOrDefaultAsync(
                        x => x.Id == sessionId,
                        cancellationToken);

            if (session == null)
                return NotFound();

            session.Status =
                "PendingOutcome";

            session.UpdatedOn =
                DateTime.UtcNow;

            await _db.SaveChangesAsync(
                cancellationToken);

            return Ok();
        }

        [HttpGet("sessions")]
        [Authorize]
        public async Task<IActionResult>
            GetSessions(
                string? search = null)
        {
            var query =
                _db.ThumbtackSessions
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query =
                    query.Where(
                        x =>
                            x.TrackingId.Contains(search) ||
                            x.UtmContent.Contains(search) ||
                            x.CategoryCode.Contains(search));
            }

            var data =
                await query
                    .Take(200)
                    .ToListAsync();

            return Ok(data);
        }
    }
}