using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.ExternalDeliveries.Thumbtack;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/public/thumbtack")]
    public sealed class PublicThumbtackController : ControllerBase
    {
        private readonly MyAppDbContext _db;
        private readonly ThumbtackApiService _thumbtackApiService;
        private readonly ThumbtackEligibilityService _eligibilityService;
        private readonly ThumbtackOptions _options;
        private readonly ILogger<PublicThumbtackController> _logger;

        public PublicThumbtackController(
            MyAppDbContext db,
            ThumbtackApiService thumbtackApiService,
            ThumbtackEligibilityService eligibilityService,
            IOptions<ThumbtackOptions> options,
            ILogger<PublicThumbtackController> logger)
        {
            _db = db;
            _thumbtackApiService = thumbtackApiService;
            _eligibilityService = eligibilityService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost("businesses")]
        public async Task<IActionResult> GetBusinesses(
            [FromBody] ThumbtackLeadRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null ||
                request.LeadId <= 0 ||
                request.LeadUuid == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    available = false,
                    message = "A valid lead reference is required."
                });
            }

            var lead = await _db.Leads
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.LeadId &&
                        x.LeadUuid == request.LeadUuid &&
                        !x.IsDeleted,
                    cancellationToken);

            if (lead == null)
            {
                return NotFound(new
                {
                    success = false,
                    available = false,
                    message = "Lead was not found."
                });
            }

            if (!lead.IsCompleted)
            {
                return Ok(new
                {
                    success = true,
                    available = false,
                    message = "Thumbtack is unavailable for an incomplete lead."
                });
            }

            /*
             * Keep the same fraud protection used by the existing
             * distribution system.
             */
            if (!lead.IsTest &&
                (
                    lead.IsSuspicious ||
                    lead.IsInvalidEmail ||
                    lead.FraudScore >= 80
                ))
            {
                return Ok(new
                {
                    success = true,
                    available = false,
                    message =
                        "Thumbtack recommendations are unavailable for this lead."
                });
            }

            /*
             * LeadTypeId cannot identify the Thumbtack service because
             * Window, Bathroom and Gutter currently all map to LeadTypeId 2.
             */
            var verticalCode = ResolveThumbtackVertical(lead);

            if (verticalCode == null)
            {
                return Ok(new
                {
                    success = true,
                    available = false,
                    message =
                        "Thumbtack is not available for the selected service."
                });
            }

            var verticalOptions =
                GetVerticalOptions(verticalCode);

            if (verticalOptions == null ||
                !verticalOptions.Enabled ||
                string.IsNullOrWhiteSpace(verticalOptions.CategoryPk))
            {
                return Ok(new
                {
                    success = true,
                    available = false,
                    message =
                        $"Thumbtack is disabled for {verticalCode}."
                });
            }

            var zipCode =
                (lead.Postcode ?? string.Empty).Trim();

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    zipCode,
                    @"^\d{5}$"))
            {
                return Ok(new
                {
                    success = true,
                    available = false,
                    message =
                        "A verified five-digit ZIP code is required."
                });
            }

            /*
             * The same lead may request this endpoint again after a refresh.
             * It must not consume the weekly cap more than once.
             */
            var existingRecord =
                await _db.ExternalLeadDeliveries
                    .FirstOrDefaultAsync(
                        x =>
                            x.LeadId == lead.Id &&
                            x.PlatformCode == "THUMBTACK",
                        cancellationToken);

            if (existingRecord == null)
            {
                var eligibility =
                    await _eligibilityService.CheckAsync(
                        lead,
                        verticalCode,
                        cancellationToken);

                if (!eligibility.Allowed)
                {
                    return Ok(new
                    {
                        success = true,
                        available = false,
                        message =
                            eligibility.Reason ??
                            "Thumbtack is unavailable for this lead."
                    });
                }
            }

            var trackingId =
                $"HOMEYY-{verticalCode.ToUpperInvariant()}-{lead.LeadUuid:N}";

            try
            {
                var result =
                    await _thumbtackApiService.SearchBusinessesAsync(
                        verticalOptions.CategoryPk,
                        zipCode,
                        trackingId,
                        10,
                        cancellationToken);

                if (result.Data.Count == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        available = false,
                        service = verticalCode,
                        categoryId = verticalOptions.CategoryPk,
                        message =
                            "No Thumbtack professionals were found for this ZIP code."
                    });
                }

                var now = DateTime.UtcNow;

                if (existingRecord == null)
                {
                    existingRecord =
                        new ExternalLeadDelivery
                        {
                            LeadId = lead.Id,
                            PlatformCode = "THUMBTACK",
                            VerticalCode = verticalCode,

                            /*
                             * ExternalLeadDeliveryWorker only processes
                             * Pending, RetryScheduled and stale Processing
                             * records. It will ignore Available.
                             */
                            Status = "Available",

                            AttemptCount = 1,
                            MaxAttempts = 1,
                            LastAttemptOn = now,
                            NextAttemptOn = null,
                            CreatedOn = now,
                            UpdatedOn = now
                        };

                    _db.ExternalLeadDeliveries.Add(
                        existingRecord);
                }
                else
                {
                    existingRecord.VerticalCode =
                        verticalCode;

                    existingRecord.Status =
                        "Available";

                    existingRecord.AttemptCount =
                        Math.Max(
                            existingRecord.AttemptCount,
                            1);

                    existingRecord.MaxAttempts =
                        Math.Max(
                            existingRecord.MaxAttempts,
                            1);

                    existingRecord.LastAttemptOn =
                        now;

                    existingRecord.NextAttemptOn =
                        null;

                    existingRecord.ErrorMessage =
                        null;

                    existingRecord.UpdatedOn =
                        now;
                }

                existingRecord.HttpStatusCode =
                    StatusCodes.Status200OK;

                existingRecord.ExternalReferenceId =
                    result.SearchId;

                existingRecord.RequestPayload =
                    JsonSerializer.Serialize(
                        new
                        {
                            leadId = lead.Id,
                            leadUuid = lead.LeadUuid,
                            service = verticalCode,
                            categoryId =
                                verticalOptions.CategoryPk,
                            zipCode,
                            trackingId
                        });

                existingRecord.ResponsePayload =
                    JsonSerializer.Serialize(result);

                await _db.SaveChangesAsync(
                    cancellationToken);

                return Ok(new
                {
                    success = true,
                    available = true,

                    environment =
                        _options.UseStaging
                            ? "Staging"
                            : "Production",

                    leadId = lead.Id,
                    leadUuid = lead.LeadUuid,
                    service = verticalCode,
                    categoryId =
                        verticalOptions.CategoryPk,
                    zipCode,
                    trackingId,
                    searchId = result.SearchId,
                    count = result.Data.Count,

                    businesses =
                        result.Data.Select(
                            x => new
                            {
                                x.BusinessId,
                                x.BusinessName,
                                x.BusinessIntroduction,
                                x.BusinessLocation,
                                x.BusinessImageUrl,
                                x.Rating,
                                x.NumberOfReviews,
                                x.YearsInBusiness,
                                x.NumberOfHires,
                                x.IsTopPro,

                                requestFlowUrl =
                                    x.Widgets
                                        ?.RequestFlowUrl,

                                servicePageUrl =
                                    x.Widgets
                                        ?.ServicePageUrl
                            }),

                    metadata = result.Metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Thumbtack recommendations failed. LeadId={LeadId}, Vertical={Vertical}.",
                    lead.Id,
                    verticalCode);

                /*
                 * The Homeyy lead is already safely stored.
                 * A Thumbtack outage must not fail or alter that lead.
                 */
                return Ok(new
                {
                    success = true,
                    available = false,
                    message =
                        "Thumbtack recommendations are temporarily unavailable."
                });
            }
        }

        private string? ResolveThumbtackVertical(
            Lead lead)
        {
            var serviceCode =
                ReadServiceCodeFromAdditionalData(
                    lead.AdditionalDataJson);

            if (string.IsNullOrWhiteSpace(serviceCode))
            {
                serviceCode =
                    FirstSupportedService(
                        lead.CampaignName,
                        lead.PageName);
            }

            return NormalizeThumbtackVertical(
                serviceCode);
        }

        private static string?
            ReadServiceCodeFromAdditionalData(
                string? additionalDataJson)
        {
            if (string.IsNullOrWhiteSpace(
                    additionalDataJson))
            {
                return null;
            }

            try
            {
                using var document =
                    JsonDocument.Parse(
                        additionalDataJson);

                var root =
                    document.RootElement;

                /*
                 * ServiceQuote and ServicesQuote forms store serviceCode.
                 */
                if (root.TryGetProperty(
                        "serviceCode",
                        out var serviceCodeElement))
                {
                    var value =
                        serviceCodeElement.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                /*
                 * Contact form stores the selected service as "service".
                 */
                if (root.TryGetProperty(
                        "service",
                        out var serviceElement))
                {
                    var value =
                        serviceElement.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }

                return null;
            }
            catch (JsonException)
            {
                /*
                 * Historical lead data may contain non-JSON values.
                 * CampaignName/PageName fallback is used in that case.
                 */
                return null;
            }
        }

        private static string?
            FirstSupportedService(
                params string?[] values)
        {
            foreach (var value in values)
            {
                var resolved =
                    NormalizeThumbtackVertical(
                        value);

                if (resolved != null)
                    return value;
            }

            return null;
        }

        private static string?
            NormalizeThumbtackVertical(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized =
                value
                    .Trim()
                    .Replace(" ", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace("&", "and")
                    .ToLowerInvariant();

            if (normalized.Contains("roof"))
                return "Roofing";

            if (normalized.Contains("window"))
                return "Windows";

            if (normalized.Contains("bath"))
                return "Bathroom";

            if (normalized.Contains("gutter"))
                return "Gutters";

            return null;
        }

        private ThumbtackVerticalOptions?
            GetVerticalOptions(
                string verticalCode)
        {
            return verticalCode switch
            {
                "Roofing" =>
                    _options.Roofing,

                "Windows" =>
                    _options.Windows,

                "Bathroom" =>
                    _options.Bathroom,

                "Gutters" =>
                    _options.Gutters,

                _ => null
            };
        }
    }

    public sealed class ThumbtackLeadRequest
    {
        public long LeadId { get; set; }

        public Guid LeadUuid { get; set; }
    }
}