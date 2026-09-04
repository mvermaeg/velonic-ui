using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.ExternalDeliveries.Thumbtack;

namespace MyApp.Api.Services.ExternalDeliveries
{
    public class ExternalLeadDistributionService
    {
        private readonly MyAppDbContext _db;
        private readonly ILogger<ExternalLeadDistributionService> _logger;
        private readonly ThumbtackEligibilityService _thumbtackEligibility;


        public ExternalLeadDistributionService(
     MyAppDbContext db,
     ILogger<ExternalLeadDistributionService> logger,
     ThumbtackEligibilityService thumbtackEligibility)
        {
            _db = db;
            _logger = logger;
            _thumbtackEligibility = thumbtackEligibility;
        }

        // ============================================================
        // NORMAL / PRODUCTION QUEUE
        // ============================================================

        public async Task QueueForLeadAsync(
            long leadId,
            CancellationToken cancellationToken = default)
        {
            var lead = await GetLeadAsync(
                leadId,
                cancellationToken);

            if (lead == null)
                return;

            if (!lead.IsCompleted)
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}: lead is incomplete.",
                    leadId);

                return;
            }

            /*
             * Keep existing production protection.
             *
             * Test leads may pass because providers can use their
             * configured test/staging environments.
             */
            if (!lead.IsTest &&
                (
                    lead.IsSuspicious ||
                    lead.IsInvalidEmail ||
                    lead.FraudScore >= 80
                ))
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}. " +
                    "IsSuspicious={IsSuspicious}, " +
                    "IsInvalidEmail={IsInvalidEmail}, " +
                    "FraudScore={FraudScore}.",
                    leadId,
                    lead.IsSuspicious,
                    lead.IsInvalidEmail,
                    lead.FraudScore);

                return;
            }

            var verticalCode =
                ResolveVertical(lead);

            if (verticalCode == null)
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}: unsupported vertical.",
                    leadId);

                return;
            }

            await QueueAllApplicablePlatformsAsync(
                lead.Id,
                verticalCode,
                cancellationToken);
        }

        // ============================================================
        // MANUAL TEST QUEUE
        //
        // This is specifically for controlled integration testing.
        // It DOES NOT weaken normal production fraud protection.
        // ============================================================

        public async Task QueueNewPlatformsForTestAsync(
            long leadId,
            CancellationToken cancellationToken = default)
        {
            var lead = await GetLeadAsync(
                leadId,
                cancellationToken);

            if (lead == null)
            {
                throw new InvalidOperationException(
                    $"Lead {leadId} was not found.");
            }

            if (!lead.IsCompleted)
            {
                throw new InvalidOperationException(
                    $"Lead {leadId} is incomplete.");
            }

            var verticalCode =
                ResolveVertical(lead);

            if (verticalCode == null)
            {
                throw new InvalidOperationException(
                    $"Unable to determine vertical for Lead {leadId}.");
            }

            /*
             * IMPORTANT:
             *
             * This manual method queues ONLY the three new integrations.
             *
             * InsuranceTales = Windows only
             * Modernize      = Roofing / Windows / HVAC / Bathroom
             * Mili           = Roofing / Windows / HVAC / Bathroom
             */

            if (string.Equals(
                    verticalCode,
                    "Windows",
                    StringComparison.OrdinalIgnoreCase))
            {
                await QueuePlatformAsync(
                    lead.Id,
                    "InsuranceTales",
                    verticalCode,
                    cancellationToken);
            }

            if (IsHomeImprovementVertical(verticalCode))
            {
                await QueuePlatformAsync(
                    lead.Id,
                    "Modernize",
                    verticalCode,
                    cancellationToken);

                await QueuePlatformAsync(
                    lead.Id,
                    "Mili",
                    verticalCode,
                    cancellationToken);
            }

            _logger.LogInformation(
                "Manual external integration test queued. LeadId={LeadId}, Vertical={Vertical}.",
                lead.Id,
                verticalCode);
        }

        // ============================================================
        // NORMAL PLATFORM DISTRIBUTION
        // ============================================================

        private async Task QueueAllApplicablePlatformsAsync(
            long leadId,
            string verticalCode,
            CancellationToken cancellationToken)
        {
            /*
             * Existing platforms.
             * Keep these exactly as existing behavior.
             */

            await QueuePlatformAsync(
                leadId,
                "BLUEINK",
                verticalCode,
                cancellationToken);

            await QueuePlatformAsync(
                leadId,
                "NETWORX",
                verticalCode,
                cancellationToken);




            if (IsThumbtackVertical(verticalCode))
            {
                var lead = await _db.Leads
                    .FirstAsync(x => x.Id == leadId, cancellationToken);

                var thumbtackCheck =
                    await _thumbtackEligibility.CheckAsync(
                        lead,
                        verticalCode,
                        cancellationToken);

                if (thumbtackCheck.Allowed)
                {
                    await QueuePlatformAsync(
                        leadId,
                        "THUMBTACK",
                        verticalCode,
                        cancellationToken);
                }
                else
                {
                    _logger.LogInformation(
                        "Thumbtack skipped for LeadId {LeadId}. Reason: {Reason}",
                        leadId,
                        thumbtackCheck.Reason);
                }
            }


            /*
             * InsuranceTales / Alpha Living
             * Windows campaign only.
             */

            if (string.Equals(
                    verticalCode,
                    "Windows",
                    StringComparison.OrdinalIgnoreCase))
            {
                await QueuePlatformAsync(
                    leadId,
                    "InsuranceTales",
                    verticalCode,
                    cancellationToken);
            }

            /*
             * Modernize
             * Current supported Homeyy integrations:
             * Roofing, Windows, HVAC, Bathroom
             */

            if (IsHomeImprovementVertical(verticalCode))
            {
                await QueuePlatformAsync(
                    leadId,
                    "Modernize",
                    verticalCode,
                    cancellationToken);
            }

            /*
             * MILI
             * Roofing, Windows, HVAC, Bathroom
             */

            if (IsHomeImprovementVertical(verticalCode))
            {
                await QueuePlatformAsync(
                    leadId,
                    "Mili",
                    verticalCode,
                    cancellationToken);
            }
        }

        // ============================================================
        // CREATE DELIVERY QUEUE ROW
        // ============================================================


        private static bool IsThumbtackVertical(
    string verticalCode)
        {
            return
                string.Equals(
                    verticalCode,
                    "Roofing",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "Windows",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "Bathroom",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "Gutters",
                    StringComparison.OrdinalIgnoreCase);
        }


        private async Task QueuePlatformAsync(
            long leadId,
            string platformCode,
            string verticalCode,
            CancellationToken cancellationToken)
        {
            var alreadyExists =
                await _db.ExternalLeadDeliveries
                    .AnyAsync(
                        x =>
                            x.LeadId == leadId &&
                            x.PlatformCode == platformCode,
                        cancellationToken);

            if (alreadyExists)
            {
                _logger.LogInformation(
                    "External delivery already exists. LeadId={LeadId}, Platform={Platform}.",
                    leadId,
                    platformCode);

                return;
            }

            var now =
                DateTime.UtcNow;

            var delivery =
                new ExternalLeadDelivery
                {
                    LeadId =
                        leadId,

                    PlatformCode =
                        platformCode,

                    VerticalCode =
                        verticalCode,

                    Status =
                        "Pending",

                    AttemptCount =
                        0,

                    MaxAttempts =
                        5,

                    NextAttemptOn =
                        now,

                    CreatedOn =
                        now,

                    UpdatedOn =
                        now
                };

            _db.ExternalLeadDeliveries.Add(
                delivery);

            try
            {
                await _db.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "External delivery queued. LeadId={LeadId}, Platform={Platform}, Vertical={Vertical}.",
                    leadId,
                    platformCode,
                    verticalCode);
            }
            catch (DbUpdateException)
            {
                /*
                 * Handles concurrent attempts when a unique index
                 * already protects LeadId + PlatformCode.
                 */

                var existsNow =
                    await _db.ExternalLeadDeliveries
                        .AnyAsync(
                            x =>
                                x.LeadId == leadId &&
                                x.PlatformCode == platformCode,
                            cancellationToken);

                if (!existsNow)
                    throw;
            }
        }

        // ============================================================
        // LOAD LEAD
        // ============================================================

        private async Task<Lead?> GetLeadAsync(
            long leadId,
            CancellationToken cancellationToken)
        {
            return await _db.Leads
                .Include(x => x.LeadType)
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == leadId &&
                        !x.IsDeleted,
                    cancellationToken);
        }

        // ============================================================
        // SUPPORTED NEW PLATFORM VERTICALS
        // ============================================================

        private static bool IsHomeImprovementVertical(
            string verticalCode)
        {
            return
                string.Equals(
                    verticalCode,
                    "Roofing",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "Windows",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "HVAC",
                    StringComparison.OrdinalIgnoreCase)
                ||
                string.Equals(
                    verticalCode,
                    "Bathroom",
                    StringComparison.OrdinalIgnoreCase);
        }

        // ============================================================
        // RESOLVE HOMEYY VERTICAL
        // ============================================================

        private static string? ResolveVertical(
            Lead lead)
        {
            /*
             * Prefer LeadTypeId because it is deterministic.
             *
             * Existing Homeyy mapping:
             *
             * 1 = Roofing
             * 2 = Windows
             * 4 = HVAC
             * 5 = Bathroom
             */

            switch (lead.LeadTypeId)
            {
                case 1:
                    return "Roofing";

                case 2:
                    return "Windows";

                case 4:
                    return "HVAC";

                case 5:
                    return "Bathroom";
            }

            /*
             * Fallback for historical leads where LeadTypeId
             * may not have been populated.
             */

            var possibleValues =
                new[]
                {
                    lead.LeadType?.Name,
                    lead.CampaignName,
                    lead.PageName
                };

            foreach (var item in possibleValues)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var normalized =
                    item
                        .Trim()
                        .Replace(" ", string.Empty)
                        .Replace("-", string.Empty)
                        .Replace("_", string.Empty)
                        .ToLowerInvariant();

                if (normalized.Contains("roof"))
                    return "Roofing";

                if (normalized.Contains("window"))
                    return "Windows";

                if (normalized.Contains("bath"))
                    return "Bathroom";

                if (
                    normalized.Contains("hvac") ||
                    normalized.Contains("heating") ||
                    normalized.Contains("cooling") ||
                    normalized.Contains("furnace") ||
                    normalized.Contains("airconditioning")
                )
                {
                    return "HVAC";
                }
            }

            return null;
        }
    }
}   