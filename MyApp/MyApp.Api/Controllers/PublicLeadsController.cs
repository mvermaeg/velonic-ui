using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.DTOs.Leads;
using MyApp.Api.DTOs.Location;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.ExternalDeliveries;
using MyApp.Api.Services.Fraud;
using MyApp.Api.Services.Location;
using System.Text.Json;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/leads")]
    public class PublicLeadsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ExternalLeadDistributionService _externalLeadDistributionService;
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
            BouncerEmailValidationService emailValidation,
            ExternalLeadDistributionService externalLeadDistributionService,
            IConfiguration configuration)
        {
            _db = db;
            _leadBiddingService = leadBiddingService;
            _leadFraudService = leadFraudService;
            _addressValidationService = addressValidationService;
            _ipLocationService = ipLocationService;
            _leadDeliveryService = leadDeliveryService;
            _emailValidation = emailValidation;
            _externalLeadDistributionService = externalLeadDistributionService;
            _configuration = configuration;
        }

        [HttpPost("validate-email")]
        public async Task<IActionResult> ValidateEmail(
    [FromBody] WebsiteEmailValidationDto model)
        {
            if (string.IsNullOrWhiteSpace(model?.Email))
            {
                return BadRequest(new
                {
                    isValid = false,
                    message = "Email address is required."
                });
            }

            var email = model.Email.Trim().ToLowerInvariant();

            var emailFormatOk =
                System.Text.RegularExpressions.Regex.IsMatch(
                    email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (!emailFormatOk)
            {
                return BadRequest(new
                {
                    isValid = false,
                    message = "Please enter a valid email address."
                });
            }

            var emailCheck =
                await _emailValidation.VerifyAsync(email);

            if (emailCheck == null)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        isValid = false,
                        message =
                            "Email verification is temporarily unavailable."
                    });
            }

            if (string.Equals(
                    emailCheck.Status,
                    "ProviderError",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        isValid = false,
                        message =
                            "Email verification is temporarily unavailable.",
                        reason = emailCheck.Reason
                    });
            }

            var accepted =
                string.Equals(
                    emailCheck.Status,
                    "deliverable",
                    StringComparison.OrdinalIgnoreCase) &&
                emailCheck.Score >= 80;

            if (!accepted)
            {
                return BadRequest(new
                {
                    isValid = false,
                    message =
                        "We could not verify this email. Please use a valid email address.",
                    status = emailCheck.Status,
                    score = emailCheck.Score,
                    reason = emailCheck.Reason
                });
            }

            return Ok(new
            {
                isValid = true,
                message = "Email address verified.",
                status = emailCheck.Status,
                score = emailCheck.Score
            });
        }


        [HttpPost("validate-address")]
        public async Task<IActionResult> ValidateAddress(
            [FromBody] WebsiteAddressValidationDto model)
        {
            if (string.IsNullOrWhiteSpace(model?.Address))
            {
                return BadRequest(new
                {
                    isValid = false,
                    message = "Full address is required."
                });
            }

            if (string.IsNullOrWhiteSpace(model.Postcode))
            {
                return BadRequest(new
                {
                    isValid = false,
                    message = "ZIP code is required."
                });
            }

            var postcode = model.Postcode.Trim();

            var validUsZip =
                System.Text.RegularExpressions.Regex.IsMatch(
                    postcode,
                    @"^\d{5}(-\d{4})?$");

            if (!validUsZip)
            {
                return BadRequest(new
                {
                    isValid = false,
                    message = "Please enter a valid US ZIP code."
                });
            }

            var addressCheck =
                await _addressValidationService.VerifyAsync(
                    new VerifyAddressPostcodeRequest
                    {
                        Address = model.Address.Trim(),
                        Postcode = postcode,
                        CountryCode = "US"
                    });

            if (string.Equals(
                    addressCheck.Status,
                    "ProviderError",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        isValid = false,
                        message =
                            "Address verification is temporarily unavailable.",
                        addressStatus = addressCheck.Status
                    });
            }

            var addressAccepted =
                addressCheck.IsValid ||
                string.Equals(
                    addressCheck.Status,
                    "Valid",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    addressCheck.Status,
                    "Review",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    addressCheck.PossibleNextAction,
                    "ACCEPT",
                    StringComparison.OrdinalIgnoreCase);

            if (!addressAccepted)
            {
                return BadRequest(new
                {
                    isValid = false,
                    message =
                        addressCheck.Message ??
                        "Address and ZIP code do not match.",
                    addressStatus = addressCheck.Status,
                    postalCode = addressCheck.PostalCode,
                    city = addressCheck.City,
                    state = addressCheck.State,
                    country = addressCheck.Country
                });
            }

            return Ok(new
            {
                isValid = true,
                message = "Address and ZIP code verified.",
                postalCode =
                    addressCheck.PostalCode ?? postcode,
                city = addressCheck.City,
                state = addressCheck.State,
                country =
                    addressCheck.Country ?? "United States"
            });
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
    ? "US"
    : model.CountryCode.Trim().ToUpperInvariant();

                if (!string.Equals(
                        model.CountryCode,
                        "US",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = "Only United States leads are currently accepted."
                    });
                }

                model.CountryCode = "US";
                model.Country = "United States";

                if (string.IsNullOrWhiteSpace(model.FullName))
                    return BadRequest(new { message = "Full name is required." });

                if (string.IsNullOrWhiteSpace(model.Phone))
                    return BadRequest(new { message = "Phone number is required." });

                if (string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest(new { message = "Email address is required." });

                //if (model.LeadTypeId == null || model.LeadTypeId <= 0)
                //    return BadRequest(new { message = "Please select a service." });

                model.ServiceCode = NormalizeServiceCode(model.ServiceCode);

                if (string.IsNullOrWhiteSpace(model.ServiceCode))
                {
                    return BadRequest(new
                    {
                        message = "Please select a service."
                    });
                }

                var configuredLeadTypeId =
                    _configuration.GetValue<int?>(
                        $"HomeyyWebsiteLeads:LeadTypeIds:{model.ServiceCode}");

                if (!configuredLeadTypeId.HasValue ||
                    configuredLeadTypeId.Value <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            $"Lead type is not configured for service: {model.ServiceCode}"
                    });
                }

                model.LeadTypeId = configuredLeadTypeId.Value;

                var emailFormatOk = System.Text.RegularExpressions.Regex.IsMatch(
                    model.Email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

                if (!emailFormatOk)
                    return BadRequest(new { message = "Please enter a valid email address." });

                var emailCheck = await _emailValidation.VerifyAsync(model.Email);

                var emailAccepted =
                    emailCheck != null &&
                    string.Equals(
                        emailCheck.Status,
                        "deliverable",
                        StringComparison.OrdinalIgnoreCase) &&
                    emailCheck.Score >= 80;

                // During Swagger/local integration testing,
                // do not block the complete pipeline because of Bouncer.
                if (!model.IsTest && !emailAccepted)
                {
                    return BadRequest(new
                    {
                        message = "Email verification failed.",
                        status = emailCheck?.Status ?? "No response",
                        score = emailCheck?.Score,
                        reason = emailCheck?.Reason
                            ?? "Email is not confirmed as deliverable."
                    });
                }


                //var phoneDigits = new string(model.Phone.Where(char.IsDigit).ToArray());

                //if (phoneDigits.Length != model.Phone.Length)
                //    return BadRequest(new { message = "Phone number should contain numbers only." });

                //if (phoneDigits.Length < 7 || phoneDigits.Length > 15)
                //    return BadRequest(new { message = "Phone number should be between 7 and 15 digits." });

                //model.Phone = phoneDigits;

                var phoneDigits = new string(
    model.Phone.Where(char.IsDigit).ToArray());

                if (phoneDigits.Length == 11 &&
                    phoneDigits.StartsWith("1"))
                {
                    phoneDigits = phoneDigits[1..];
                }

                if (phoneDigits.Length != 10)
                {
                    return BadRequest(new
                    {
                        message =
                            "Please enter a valid 10-digit US phone number."
                    });
                }

                model.Phone = phoneDigits;

                // Address is required as lead information, but it is intentionally
                // NOT validated against Google and is NOT matched to the ZIP code.
                if (string.IsNullOrWhiteSpace(model.Address))
                    return BadRequest(new { message = "Full address is required." });

                if (string.IsNullOrWhiteSpace(model.Postcode))
                    return BadRequest(new { message = "ZIP code is required." });

                // Homeyy currently accepts standard 5-digit US ZIP codes.
                // Format validation alone is not enough, so the existing location
                // provider is used as a ZIP lookup by sending the ZIP as both values.
                var validUsZipFormat =
                    System.Text.RegularExpressions.Regex.IsMatch(
                        model.Postcode,
                        @"^\d{5}$");

                if (!validUsZipFormat)
                {
                    return BadRequest(new
                    {
                        message = "Please enter a valid 5-digit US ZIP code."
                    });
                }

                var zipCheck = await _addressValidationService.VerifyAsync(
                    new VerifyAddressPostcodeRequest
                    {
                        Address = model.Postcode,
                        Postcode = model.Postcode,
                        CountryCode = "US"
                    });

                if (string.Equals(
                        zipCheck.Status,
                        "ProviderError",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new
                        {
                            message = "ZIP verification is temporarily unavailable.",
                            zipStatus = zipCheck.Status
                        });
                }

                var returnedPostcode =
                    (zipCheck.PostalCode ?? string.Empty)
                    .Trim();

                if (returnedPostcode.Length > 5)
                    returnedPostcode = returnedPostcode[..5];

                var returnedCountry =
                    (zipCheck.Country ?? string.Empty)
                    .Trim();

                var zipAccepted =
                    (
                        zipCheck.IsValid ||
                        string.Equals(
                            zipCheck.Status,
                            "Valid",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            zipCheck.PossibleNextAction,
                            "ACCEPT",
                            StringComparison.OrdinalIgnoreCase)
                    ) &&
                    string.Equals(
                        returnedPostcode,
                        model.Postcode,
                        StringComparison.OrdinalIgnoreCase) &&
                    (
                        string.Equals(
                            returnedCountry,
                            "United States",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            returnedCountry,
                            "United States of America",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            returnedCountry,
                            "USA",
                            StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(
                            returnedCountry,
                            "US",
                            StringComparison.OrdinalIgnoreCase)
                    );

                if (!zipAccepted)
                {
                    return BadRequest(new
                    {
                        message = "Please enter a valid United States ZIP code.",
                        zipStatus = zipCheck.Status,
                        postalCode = zipCheck.PostalCode,
                        city = zipCheck.City,
                        state = zipCheck.State,
                        country = zipCheck.Country
                    });
                }

                // City/state/country are authoritative from the verified ZIP.
                // The user's address text is left untouched.
                model.City = zipCheck.City ?? model.City;
                model.State = zipCheck.State ?? model.State;
                model.Country = zipCheck.Country ?? "United States";
                model.Postcode = returnedPostcode;
                model.CountryCode = "US";

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

                //var lead = new Lead
                //{
                //    LeadUuid = Guid.NewGuid(),
                //    CreatedAt = DateTime.UtcNow,
                //    ReceivedAt = DateTime.UtcNow,
                //    FingerprintHash = model.FingerprintHash,
                //    FullName = model.FullName,
                //    Email = model.Email,
                //    Phone = model.Phone,
                //    IpAddress = ipAddress,
                //    UserAgent = userAgent,
                //    VisitorCountry = visitorLocation?.CountryName,
                //    CampaignName = model.CampaignName,
                //    AffiliateName = affiliateClick?.AffiliateName ?? model.AffiliateName,
                //    PageName = model.PageName,
                //    AffiliateId = affiliateClick?.AffiliateId,
                //    AffiliateSubId = affiliateSubId,
                //    AffiliateClickId = affiliateClick?.Id,
                //    AffiliateClickUuid = affiliateClick?.ClickUuid,
                //    Address = model.Address,
                //    Postcode = model.Postcode,
                //    State = model.State,
                //    City = model.City,
                //    Country = model.Country,
                //    Step = model.Step,
                //    IsTest = model.IsTest,
                //    IsCompleted = model.IsCompleted,
                //    LeadTypeId = model.LeadTypeId,
                //    LeadStatus = "New",
                //    IsDeleted = false,
                //    EmailVerificationStatus = emailCheck.Status,
                //    EmailVerificationReason = emailCheck.Reason,
                //    IsEmailDeliverable = emailCheck.Deliverable
                //};


                var source =
    _configuration["HomeyyWebsiteLeads:Source"]
    ?? "HomeyyWebsite";

                var sourceReference =
                    _configuration["HomeyyWebsiteLeads:SourceReference"]
                    ?? "homeyy.com";

                var tcpaComplianceText =
                    _configuration["HomeyyWebsiteLeads:TcpaComplianceText"];

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
                    Source = source,
                    SourceReference = sourceReference,
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

                    TcpaComplianceText =
    model.IsTcpaCompliant == true
        ? tcpaComplianceText
        : null,
                    TrustedFormCertificateUrl = model.TrustedFormCertificateUrl,
                    JornayaLeadId = model.JornayaLeadId,
                    LandingPageUrl = model.LandingPageUrl,
                    ConsentCapturedOn =
    model.IsTcpaCompliant == true
        ? DateTime.UtcNow
        : null,
                    IsTcpaCompliant = model.IsTcpaCompliant,
                    OwnsProperty = model.OwnsProperty,
                    AdditionalDataJson = model.AdditionalDataJson,

                    LeadStatus = "New",
                    IsDeleted = false,

                    EmailVerificationStatus =
    emailCheck?.Status ?? "NotVerified",

                    EmailVerificationReason =
    emailCheck?.Reason ?? "Email verification unavailable during test.",

                    IsEmailDeliverable =
    emailCheck?.Deliverable ?? false
                };


                _db.Leads.Add(lead);
                await _db.SaveChangesAsync();

                var rawPayload = new LeadRawPayload
                {
                    LeadId = lead.Id,
                    SourceName = source,
                    ExternalLeadId = null,
                    RawJson = JsonSerializer.Serialize(model),
                    ReceivedOn = DateTime.UtcNow
                };

                _db.LeadRawPayloads.Add(rawPayload);
                await _db.SaveChangesAsync();

                await _leadFraudService.CheckAndApplyAsync(lead);
                await _db.SaveChangesAsync();

                //await _leadBiddingService.RunForLeadAsync(lead.Id);

                //await _externalLeadDistributionService.QueueForLeadAsync(
                //    lead.Id);

                //var winningBid = await _db.LeadBiddingResults
                //    .Where(x => x.LeadId == lead.Id && x.IsWon && !x.IsSold)
                //    .OrderByDescending(x => x.BidAmount)
                //    .FirstOrDefaultAsync();

                //if (winningBid != null)
                //{
                //    await _leadDeliveryService
                //        .CreateDeliveryFromBiddingResultAsync(winningBid.Id);
                //}

                if (!lead.IsTest)
                {
                    await _leadBiddingService.RunForLeadAsync(lead.Id);

                    await _externalLeadDistributionService
                        .QueueForLeadAsync(lead.Id);

                    var winningBid = await _db.LeadBiddingResults
                        .Where(x =>
                            x.LeadId == lead.Id &&
                            x.IsWon &&
                            !x.IsSold)
                        .OrderByDescending(x => x.BidAmount)
                        .FirstOrDefaultAsync();

                    if (winningBid != null)
                    {
                        await _leadDeliveryService
                            .CreateDeliveryFromBiddingResultAsync(
                                winningBid.Id);
                    }
                }

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


        private static string NormalizeServiceCode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value
                .Trim()
                .ToLowerInvariant()
                .Replace("&", "and")
                .Replace(" ", "")
                .Replace("-", "");

            return normalized switch
            {
                "acandheat" => "hvac",
                "acheat" => "hvac",
                "hvacsystems" => "hvac",
                "bathroomremodeling" => "bathroom",
                "kitchenremodeling" => "kitchen",
                "plumbingservices" => "plumbing",
                "windowinstallation" => "window",
                "doorinstallation" => "door",
                "flooringservices" => "flooring",
                "gutterinstallation" => "gutter",
                "fencinginstallation" => "fencing",
                "solarinstallation" => "solar",
                "roofinginstallation" => "roofing",
                "sidinginstallation" => "siding",
                "homesecuritysystems" => "homesecurity",
                _ => normalized
            };
        }
    }
}
 
