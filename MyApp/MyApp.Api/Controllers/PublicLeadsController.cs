using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.Leads;
using MyApp.Api.Services.Bidding;
using System.Text.Json;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/leads")]
    public class PublicLeadsController : ControllerBase
    {
        private readonly LeadBiddingService _leadBiddingService;
        private readonly MyAppDbContext _db;

        public PublicLeadsController(MyAppDbContext db, LeadBiddingService leadBiddingService)
        {
            _db = db;
            _leadBiddingService = leadBiddingService;
        }

        [HttpPost("website")]
        public async Task<IActionResult> CreateWebsiteLead(WebsiteLeadCreateDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) && string.IsNullOrWhiteSpace(model.Phone))
            {
                return BadRequest(new
                {
                    message = "Email or phone is required."
                });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var affiliateSubId = !string.IsNullOrWhiteSpace(model.AffiliateSubId)
                ? model.AffiliateSubId
                : model.AffiliateSubId;

            AffiliateClick? affiliateClick = null;

            if (!string.IsNullOrWhiteSpace(affiliateSubId))
            {
                affiliateClick = await _db.AffiliateClicks
                    .Where(x => x.SubId == affiliateSubId)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();
            }
            //var lead = new Lead
            //{
            //    CreatedAt = DateTime.UtcNow,
            //    ReceivedAt = DateTime.UtcNow,

            //    FullName = model.FullName,
            //    Email = model.Email,
            //    Phone = model.Phone,

            //    IpAddress = ipAddress,

            //    CampaignName = model.CampaignName,
            //    AffiliateName = model.AffiliateName,
            //    PageName = model.PageName,

            //    Address = model.Address,
            //    Postcode = model.Postcode,
            //    State = model.State,
            //    City = model.City,

            //    Step = model.Step,
            //    IsTest = model.IsTest,
            //    IsCompleted = model.IsCompleted,

            //   // Attempts = 0,
            //    LeadStatus = "New",
            //    IsDeleted = false
            //};

            var lead = new Lead
            {
                LeadUuid = Guid.NewGuid(),

                CreatedAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow,

                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,

                IpAddress = ipAddress,

                CampaignName = model.CampaignName,
                AffiliateName = affiliateClick?.AffiliateName ?? model.AffiliateName,
                PageName = model.PageName,

                AffiliateId = affiliateClick?.AffiliateId,
                AffiliateSubId = affiliateSubId,
                AffiliateClickId = affiliateClick?.Id,
                AffiliateClickUuid = affiliateClick?.ClickUuid,

                Address = model.Address,
                Postcode = model.Postcode,
                State = model.State,
                City = model.City,

                Step = model.Step,
                IsTest = model.IsTest,
                IsCompleted = model.IsCompleted,

                LeadStatus = "New",
                IsDeleted = false
            };
            _db.Leads.Add(lead);
            await _db.SaveChangesAsync();
            await _leadBiddingService.RunForLeadAsync(lead.Id);
            var rawPayload = new LeadRawPayload
            {
                LeadId = lead.Id,
                SourceName = "Website",
                ExternalLeadId = null,
                RawJson = JsonSerializer.Serialize(model),
                ReceivedOn = DateTime.UtcNow
            };

            _db.LeadRawPayloads.Add(rawPayload);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Lead received successfully.",
                leadId = lead.Id,
                leadUuid = lead.LeadUuid
            });
        }
    }
}