using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.DTOs.LeadDeliveries;
using MyApp.Api.Services.Deliveries;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/lead-deliveries")]
    [Authorize]
    public class LeadDeliveriesController : ControllerBase
    {
        private readonly MyAppDbContext _db;
        private readonly LeadDeliveryService _deliveryService;

        public LeadDeliveriesController(
            MyAppDbContext db,
            LeadDeliveryService deliveryService)
        {
            _db = db;
            _deliveryService = deliveryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDeliveries(
            [FromQuery] long? clientId,
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query =
                from delivery in _db.LeadDeliveries
                join lead in _db.Leads on delivery.LeadId equals lead.Id
                join client in _db.Clients on delivery.ClientId equals client.Id
                where lead.IsDeleted == false && client.IsDeleted == false
                select new
                {
                    delivery,
                    lead,
                    client
                };

            if (clientId.HasValue)
            {
                query = query.Where(x => x.delivery.ClientId == clientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.delivery.DeliveryStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.lead.Email != null && x.lead.Email.Contains(search)) ||
                    (x.lead.FullName != null && x.lead.FullName.Contains(search)) ||
                    (x.lead.CampaignName != null && x.lead.CampaignName.Contains(search)) ||
                    x.client.ClientName.Contains(search));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.delivery.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeadDeliveryDto
                {
                    Id = x.delivery.Id,
                    LeadId = x.delivery.LeadId,
                    LeadUuid = x.lead.LeadUuid,
                    ClientId = x.delivery.ClientId,
                    ClientName = x.client.ClientName,
                    LeadBiddingResultId = x.delivery.LeadBiddingResultId,
                    DeliveryType = x.delivery.DeliveryType,
                    DeliveryStatus = x.delivery.DeliveryStatus,
                    DeliveredOn = x.delivery.DeliveredOn,
                    ResponseMessage = x.delivery.ResponseMessage,
                    CreatedOn = x.delivery.CreatedOn,
                    Email = x.lead.Email,
                    CampaignName = x.lead.CampaignName,
                    SourceName = x.lead.AffiliateName
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

        [HttpPost("from-bidding/{biddingResultId}")]
        public async Task<IActionResult> CreateFromBidding(long biddingResultId)
        {
            try
            {
                var deliveryId = await _deliveryService.CreateDeliveryFromBiddingResultAsync(biddingResultId);

                return Ok(new
                {
                    message = "Lead delivered successfully.",
                    deliveryId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}