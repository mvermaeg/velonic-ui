using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.ClientBidding;
using MyApp.Api.Services.Deliveries;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/client-bidding")]
    [Authorize]
    public class ClientBiddingController : ControllerBase
    {
        private readonly LeadDeliveryService _deliveryService;
        private readonly MyAppDbContext _db;

        public ClientBiddingController(MyAppDbContext db, LeadDeliveryService deliveryService)
        {
            _db = db;
            _deliveryService = deliveryService;
        }

        [HttpPost("results/{id}/deliver")]
        public async Task<IActionResult> DeliverResult(long id)
        {
            try
            {
                var deliveryId = await _deliveryService.CreateDeliveryFromBiddingResultAsync(id);

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

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetByClient(long clientId)
        {
            var data = await _db.ClientBiddingSettings
                .Where(x => x.ClientId == clientId)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClientBiddingSettingDto model)
        {
            var clientExists = await _db.Clients.AnyAsync(x =>
                x.Id == model.ClientId && x.IsDeleted == false);

            if (!clientExists)
                return BadRequest(new { message = "Invalid client selected." });

            if (model.BidAmount <= 0)
                return BadRequest(new { message = "Bid amount must be greater than zero." });

            var setting = new ClientBiddingSetting
            {
                ClientId = model.ClientId,
                LeadType = model.LeadType,
                State = model.State,
                Postcode = model.Postcode,
                BidAmount = model.BidAmount,
                DailyCap = model.DailyCap,
                MonthlyCap = model.MonthlyCap,
                IsExclusive = model.IsExclusive,
                IsActive = model.IsActive,
                CreatedOn = DateTime.UtcNow
            };

            _db.ClientBiddingSettings.Add(setting);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Client bidding setting created successfully.",
                id = setting.Id
            });
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromQuery] bool isActive)
        {
            var setting = await _db.ClientBiddingSettings.FindAsync(id);

            if (setting == null)
                return NotFound(new { message = "Bidding setting not found." });

            setting.IsActive = isActive;
            setting.UpdatedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Bidding setting status updated successfully." });
        }

        [HttpGet("results")]
        public async Task<IActionResult> GetBiddingResults(
    [FromQuery] long? clientId,
    [FromQuery] string? search,
    [FromQuery] DateTime? fromDate,
    [FromQuery] DateTime? toDate,
    [FromQuery] bool excludeAfterSale = true,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query =
                from br in _db.LeadBiddingResults
                join lead in _db.Leads on br.LeadId equals lead.Id
                join client in _db.Clients on br.ClientId equals client.Id
                where lead.IsDeleted == false && client.IsDeleted == false
                select new
                {
                    br,
                    lead,
                    client
                };

            if (clientId.HasValue)
            {
                query = query.Where(x => x.br.ClientId == clientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.lead.Email != null && x.lead.Email.Contains(search)) ||
                    (x.lead.FullName != null && x.lead.FullName.Contains(search)) ||
                    (x.lead.CampaignName != null && x.lead.CampaignName.Contains(search)) ||
                    (x.client.ClientName != null && x.client.ClientName.Contains(search)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.br.CreatedOn >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.br.CreatedOn <= toDate.Value);
            }

            if (excludeAfterSale)
            {
                query = query.Where(x => x.br.IsSold == false);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.br.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LeadBiddingResultDto
                {
                    Id = x.br.Id,
                    LeadId = x.br.LeadId,
                    LeadUuid = x.lead.LeadUuid,
                    ClientId = x.br.ClientId,
                    ClientName = x.client.ClientName,
                    CreatedOn = x.br.CreatedOn,
                    LeadCreatedAt = x.lead.CreatedAt,
                    Email = x.lead.Email,
                    CampaignName = x.lead.CampaignName,
                    SourceName = x.lead.AffiliateName,
                    BidAmount = x.br.BidAmount,
                    IsWon = x.br.IsWon,
                    IsSold = x.br.IsSold,
                    SoldOn = x.br.SoldOn,
                    MatchReason = x.br.MatchReason
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