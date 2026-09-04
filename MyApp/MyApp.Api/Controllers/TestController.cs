using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Services.ExternalDeliveries;
using System.Data;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/test")]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public TestController(MyAppDbContext db)
        {
            _db = db;
        }



        [HttpGet("by-lead/{leadId:long}")]
        public async Task<IActionResult> GetByLead(long leadId)
        {
            var deliveries = await _db.ExternalLeadDeliveries
                .AsNoTracking()
                .Where(x => x.LeadId == leadId)
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.LeadId,
                    x.PlatformCode,
                    x.VerticalCode,
                    x.Status,
                    x.AttemptCount,
                    x.MaxAttempts,
                    x.HttpStatusCode,
                    x.ExternalReferenceId,
                    x.ErrorMessage,
                    x.RequestPayload,
                    x.ResponsePayload,
                    x.CreatedOn,
                    x.LastAttemptOn,
                    x.DeliveredOn
                })
                .ToListAsync();

            return Ok(new
            {
                leadId,
                count = deliveries.Count,
                deliveries
            });
        }


        [HttpPost("queue-new-platforms/{leadId:long}")]
        public async Task<IActionResult> QueueNewPlatforms(
    long leadId,
    [FromServices] ExternalLeadDistributionService distributionService,
    CancellationToken cancellationToken)
        {
            try
            {
                await distributionService.QueueNewPlatformsForTestAsync(
                    leadId,
                    cancellationToken);

                return Ok(new
                {
                    success = true,
                    leadId,
                    message =
                        "Lead queued for applicable new external platforms."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    leadId,
                    message = ex.Message
                });
            }
        }




        [HttpGet("database")]
        public async Task<IActionResult> Database()
        {
            var connection = _db.Database.GetDbConnection();

            try
            {
                var serverBeforeOpen = connection.DataSource;
                var databaseBeforeOpen = connection.Database;

                if (connection.State != System.Data.ConnectionState.Open)
                    await connection.OpenAsync();

                return Ok(new
                {
                    success = true,
                    server = connection.DataSource,
                    database = connection.Database,
                    connectionState = connection.State.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,

                    // MOST IMPORTANT INFORMATION
                    serverBeingUsed = connection.DataSource,
                    databaseBeingUsed = connection.Database,

                    message = ex.Message,
                    innerMessage = ex.InnerException?.Message,
                    baseMessage = ex.GetBaseException().Message
                });
            }
        }
    }
}