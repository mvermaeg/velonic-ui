using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/lead-sales")]
    [Authorize]
    public class LeadSalesController : ControllerBase
    {
        private readonly MyAppDbContext _db;

        public LeadSalesController(MyAppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetSales(
            [FromQuery] string? search,
            [FromQuery] long? clientId,
            [FromQuery] string? campaignName,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query =
                from delivery in _db.LeadDeliveries
                join lead in _db.Leads on delivery.LeadId equals lead.Id
                join client in _db.Clients on delivery.ClientId equals client.Id
                join bid in _db.LeadBiddingResults on delivery.LeadBiddingResultId equals bid.Id into bidJoin
                from bid in bidJoin.DefaultIfEmpty()
                where lead.IsDeleted == false
                      && client.IsDeleted == false
                      && delivery.DeliveryStatus == "Delivered"
                select new
                {
                    delivery,
                    lead,
                    client,
                    bid
                };

            if (clientId.HasValue)
                query = query.Where(x => x.client.Id == clientId.Value);

            if (!string.IsNullOrWhiteSpace(campaignName))
                query = query.Where(x => x.lead.CampaignName == campaignName);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.lead.Email != null && x.lead.Email.Contains(search)) ||
                    (x.lead.FullName != null && x.lead.FullName.Contains(search)) ||
                    (x.lead.CampaignName != null && x.lead.CampaignName.Contains(search)) ||
                    x.client.ClientName.Contains(search));
            }

            var total = await query.CountAsync();

            var totalRevenue = await query.SumAsync(x => x.bid != null ? x.bid.BidAmount : 0);

            var data = await query
                .OrderByDescending(x => x.delivery.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    id = x.delivery.Id,
                    createdOn = x.delivery.CreatedOn,
                    deliveredOn = x.delivery.DeliveredOn,

                    leadId = x.lead.Id,
                    leadUuid = x.lead.LeadUuid,
                    fullName = x.lead.FullName,
                    email = x.lead.Email,
                    phone = x.lead.Phone,
                    campaignName = x.lead.CampaignName,
                    sourceName = x.lead.AffiliateName,
                    state = x.lead.State,
                    city = x.lead.City,
                    postcode = x.lead.Postcode,

                    clientId = x.client.Id,
                    clientName = x.client.ClientName,

                    saleAmount = x.bid != null ? x.bid.BidAmount : 0,
                    deliveryStatus = x.delivery.DeliveryStatus,
                    responseMessage = x.delivery.ResponseMessage
                })
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                totalRevenue,
                data
            });
        }
    }
}