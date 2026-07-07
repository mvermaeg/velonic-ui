using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.Leads;
using MyApp.Api.DTOs.Location;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.Fraud;
using MyApp.Api.Services.Location;
using System.Text.Json;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/leads")]
    public class PublicLeadsController : ControllerBase
    {
        private readonly BouncerEmailValidationService _emailValidation;
        private readonly LeadDeliveryService _leadDeliveryService;
        private readonly IpLocationService _ipLocationService;
        private readonly LeadBiddingService _leadBiddingService;
        private readonly MyAppDbContext _db;
        private readonly ILeadFraudService _leadFraudService;
        private readonly IAddressValidationService _addressValidationService;

        public PublicLeadsController(
            MyAppDbContext db,
            LeadBiddingService leadBiddingService,
            ILeadFraudService leadFraudService,
            IpLocationService ipLocationService,
            IAddressValidationService addressValidationService,
            LeadDeliveryService leadDeliveryService,
            BouncerEmailValidationService emailValidation)
        {
            _db = db;
            _leadBiddingService = leadBiddingService;
            _leadFraudService = leadFraudService;
            _addressValidationService = addressValidationService;
            _ipLocationService = ipLocationService;
            _leadDeliveryService = leadDeliveryService;
            _emailValidation = emailValidation;
        }

        [HttpPost("website")]
        public async Task<IActionResult> CreateWebsiteLead(WebsiteLeadCreateDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Invalid lead data." });

                model.FullName = model.FullName?.Trim();
                model.Email = model.Email?.Trim();
                model.Phone = model.Phone?.Trim();
                model.Address = model.Address?.Trim();
                model.Postcode = model.Postcode?.Trim();
                model.CountryCode = string.IsNullOrWhiteSpace(model.CountryCode)
                    ? "IN"
                    : model.CountryCode.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(model.FullName))
                    return BadRequest(new { message = "Full name is required." });

                if (string.IsNullOrWhiteSpace(model.Phone))
                    return BadRequest(new { message = "Phone number is required." });

                if (string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest(new { message = "Email address is required." });

                if (model.LeadTypeId == null || model.LeadTypeId <= 0)
                    return BadRequest(new { message = "Please select a service." });

                var emailFormatOk = System.Text.RegularExpressions.Regex.IsMatch(
                    model.Email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailFormatOk)
                    return BadRequest(new { message = "Please enter a valid email address." });

                var emailCheck = await _emailValidation.VerifyAsync(model.Email);

                if (emailCheck == null ||
                    !string.Equals(emailCheck.Status, "deliverable", StringComparison.OrdinalIgnoreCase) ||
                    emailCheck.Score < 80)
                {
                    return BadRequest(new
                    {
                        message = $"Email verification failed. Status: {emailCheck?.Status ?? "No response"}, Score: {emailCheck?.Score?.ToString() ?? "N/A"}, Reason: {emailCheck?.Reason ?? "Email is not confirmed as deliverable."}"
                    });
                }
                 

                var phoneDigits = new string(model.Phone.Where(char.IsDigit).ToArray());

                if (phoneDigits.Length != model.Phone.Length)
                    return BadRequest(new { message = "Phone number should contain numbers only." });

                if (phoneDigits.Length < 7 || phoneDigits.Length > 15)
                    return BadRequest(new { message = "Phone number should be between 7 and 15 digits." });

                model.Phone = phoneDigits;

                if (string.IsNullOrWhiteSpace(model.Address))
                    return BadRequest(new { message = "Full address is required." });

                if (string.IsNullOrWhiteSpace(model.Postcode))
                    return BadRequest(new { message = "Postcode is required." });

                var addressCheck = await _addressValidationService.VerifyAsync(
                    new VerifyAddressPostcodeRequest
                    {
                        Address = model.Address,
                        Postcode = model.Postcode,
                        CountryCode = model.CountryCode
                    });

                var addressAccepted =
                    addressCheck.IsValid ||
                    string.Equals(addressCheck.Status, "Valid", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(addressCheck.Status, "Review", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(addressCheck.PossibleNextAction, "ACCEPT", StringComparison.OrdinalIgnoreCase);

                var addressRejected =
                    string.Equals(addressCheck.Status, "Mismatch", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(addressCheck.Status, "ProviderError", StringComparison.OrdinalIgnoreCase);

                if (!addressAccepted || addressRejected)
                {
                    return BadRequest(new
                    {
                        message = addressCheck.Message ?? "Address and postcode could not be verified.",
                        addressStatus = addressCheck.Status,
                        addressCheck
                    });
                }

                model.City = addressCheck.City ?? model.City;
                model.State = addressCheck.State ?? model.State;
                model.Country = addressCheck.Country ?? model.Country;
                model.Postcode = addressCheck.PostalCode ?? model.Postcode;

                var ipAddress =
                    Request.Headers["CF-Connecting-IP"].FirstOrDefault()
                    ?? Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                    ?? HttpContext.Connection.RemoteIpAddress?.ToString();

                var visitorLocation = await _ipLocationService.GetAsync(ipAddress);
                var userAgent = Request.Headers.UserAgent.ToString();
                var affiliateSubId = model.AffiliateSubId;

                AffiliateClick? affiliateClick = null;

                if (!string.IsNullOrWhiteSpace(affiliateSubId))
                {
                    affiliateClick = await _db.AffiliateClicks
                        .Where(x => x.SubId == affiliateSubId)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefaultAsync();
                }

                var lead = new Lead
                {
                    LeadUuid = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    ReceivedAt = DateTime.UtcNow,
                    FingerprintHash = model.FingerprintHash,
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    VisitorCountry = visitorLocation?.CountryName,
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
                    Country = model.Country,
                    Step = model.Step,
                    IsTest = model.IsTest,
                    IsCompleted = model.IsCompleted,
                    LeadTypeId = model.LeadTypeId,
                    LeadStatus = "New",
                    IsDeleted = false,
                    EmailVerificationStatus = emailCheck.Status,
                    EmailVerificationReason = emailCheck.Reason,
                    IsEmailDeliverable = emailCheck.Deliverable
                };

                _db.Leads.Add(lead);
                await _db.SaveChangesAsync();

                await _leadFraudService.CheckAndApplyAsync(lead);
                await _db.SaveChangesAsync();

                await _leadBiddingService.RunForLeadAsync(lead.Id);

                var winningBid = await _db.LeadBiddingResults
                    .Where(x => x.LeadId == lead.Id && x.IsWon && !x.IsSold)
                    .OrderByDescending(x => x.BidAmount)
                    .FirstOrDefaultAsync();

                if (winningBid != null)
                    await _leadDeliveryService.CreateDeliveryFromBiddingResultAsync(winningBid.Id);

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
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lead submit failed.",
                    error = ex.Message,
                    innerMessage = ex.InnerException?.Message
                });
            }
        }
    }
}
