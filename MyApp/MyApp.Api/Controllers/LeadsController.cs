using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.DTOs.Leads;
using MyApp.Api.Services.Fraud;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeadsController : ControllerBase
    {
        private readonly MyAppDbContext _db;
        private readonly ILeadFraudService _leadFraudService;
        public LeadsController(MyAppDbContext db, ILeadFraudService leadFraudService)
        {
            _db = db;
            _leadFraudService = leadFraudService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeads(
          [FromQuery] string? search,
          [FromQuery] string? campaignName,
          [FromQuery] string? status,
          [FromQuery] string? fraudLevel,
          [FromQuery] bool? isSuspicious,
          [FromQuery] int? minQualityScore,
          [FromQuery] int page = 1,
          [FromQuery] int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.Leads
                .Where(x => x.IsDeleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    (x.FullName != null && x.FullName.Contains(search)) ||
                    (x.Email != null && x.Email.Contains(search)) ||
                    (x.Phone != null && x.Phone.Contains(search)) ||
                    (x.Postcode != null && x.Postcode.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(campaignName))
            {
                query = query.Where(x => x.CampaignName == campaignName);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.LeadStatus == status);
            }



            if (!string.IsNullOrWhiteSpace(fraudLevel))
            {
                query = query.Where(x => x.FraudLevel == fraudLevel);
            }

            if (isSuspicious.HasValue)
            {
                query = query.Where(x => x.IsSuspicious == isSuspicious.Value);
            }

            if (minQualityScore.HasValue)
            {
                query = query.Where(x => x.LeadQualityScore >= minQualityScore.Value);
            }


            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
              .Select(x => new LeadListDto
              {
                  Id = x.Id,
                  LeadUuid = x.LeadUuid,
                  CreatedAt = x.CreatedAt,
                  ReceivedAt = x.ReceivedAt,

                  FullName = x.FullName,
                  Email = x.Email,
                  Phone = x.Phone,
                  VisitorCountry = x.VisitorCountry,
                  FingerprintHash = x.FingerprintHash,
                  IsVoipNumber = x.IsVoipNumber,
                  CampaignName = x.CampaignName,
                  AffiliateName = x.AffiliateName,
                  PageName = x.PageName,

                  Postcode = x.Postcode,
                  State = x.State,
                  City = x.City,
                  Country = x.Country,
                  IpAddress = x.IpAddress,

                  Step = x.Step,
                  IsCompleted = x.IsCompleted,
                  LeadStatus = x.LeadStatus,

                  FraudScore = x.FraudScore,
                  LeadQualityScore = x.LeadQualityScore,
                  FraudLevel = x.FraudLevel,
                  FraudReasons = x.FraudReasons,

                  DuplicateLeadCount = x.DuplicateLeadCount,
LastDuplicateLeadId = x.LastDuplicateLeadId,
                  IsInvalidEmail = x.IsInvalidEmail,
                  IsDisposableEmail = x.IsDisposableEmail,
                  IsDuplicateLead = x.IsDuplicateLead,
                  IsSuspicious = x.IsSuspicious,
                  IsPostcodeCountryMismatch = x.IsPostcodeCountryMismatch
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


        [HttpPost("{id:long}/run-fraud-check")]
        public async Task<IActionResult> RunFraudCheck(long id)
        {
            var lead = await _db.Leads
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (lead == null)
                return NotFound(new { message = "Lead not found." });

            await _leadFraudService.CheckAndApplyAsync(lead);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Fraud check completed.",
                lead.Id,
                lead.FraudScore,
                lead.LeadQualityScore,
                lead.FraudLevel,
                lead.FraudReasons
            });
        }



        [HttpPost("run-fraud-check-pending")]
        public async Task<IActionResult> RunFraudCheckPending()
        {
            var leads = await _db.Leads
                .Where(x => !x.IsDeleted && x.RiskCheckedOn == null)
                .OrderByDescending(x => x.Id)
                .Take(200)
                .ToListAsync();

            foreach (var lead in leads)
            {
                await _leadFraudService.CheckAndApplyAsync(lead);
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Pending fraud checks completed.",
                count = leads.Count
            });
        }
    }
}