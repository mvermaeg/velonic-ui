using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using System.Text.Json;

namespace MyApp.Api.Services.ExternalDeliveries
{
    public class ExternalLeadDistributionService
    {
        private readonly MyAppDbContext _db;
        private readonly ILogger<ExternalLeadDistributionService> _logger;
        public ExternalLeadDistributionService(
            MyAppDbContext db,
            ILogger<ExternalLeadDistributionService> logger)
        {
            _db = db;
            _logger = logger;
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
             * CONTROLLED INTEGRATION TEST:
             *
             * Queue the existing providers directly for this one lead,
             * bypassing only the normal fraud gate. Production
             * QueueForLeadAsync remains unchanged.
             *
             * BlueInk        = applicable resolved verticals
             * Networx        = applicable resolved verticals
             * InsuranceTales = Windows only
             * Modernize      = Roofing / Windows / HVAC / Bathroom
             * Mili           = Roofing / Windows / HVAC / Bathroom
             *
             * Thumbtack is not an IExternalLeadProvider. Its business-search
             * Request Flow is handled separately by ThumbtackApiService.
             */

            await QueuePlatformAsync(
                lead.Id,
                "BLUEINK",
                verticalCode,
                cancellationToken);

            await QueuePlatformAsync(
                lead.Id,
                "NETWORX",
                verticalCode,
                cancellationToken);

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
             * HomeyyWebsiteLeads intentionally reuses LeadTypeId 2 for
             * several services. Therefore serviceCode/service from the
             * saved website payload must be checked before LeadTypeId.
             */

            var payloadService =
                ReadServiceFromAdditionalData(
                    lead.AdditionalDataJson);

            var payloadVertical =
                NormalizeSupportedVertical(
                    payloadService);

            if (payloadVertical != null)
                return payloadVertical;

            var possibleValues =
                new[]
                {
                    lead.CampaignName,
                    lead.PageName,
                    lead.LeadType?.Name
                };

            foreach (var item in possibleValues)
            {
                var vertical =
                    NormalizeSupportedVertical(item);

                if (vertical != null)
                    return vertical;
            }

            /*
             * Legacy fallback only. LeadTypeId 2 is deliberately not
             * mapped because it is shared by multiple website services.
             */

            switch (lead.LeadTypeId)
            {
                case 1:
                    return "Roofing";

                case 4:
                    return "HVAC";

                case 5:
                    return "Bathroom";
            }

            return null;
        }

        private static string? ReadServiceFromAdditionalData(
            string? additionalDataJson)
        {
            if (string.IsNullOrWhiteSpace(additionalDataJson))
                return null;

            try
            {
                using var document =
                    JsonDocument.Parse(additionalDataJson);

                var root = document.RootElement;

                if (root.ValueKind != JsonValueKind.Object)
                    return null;

                if (root.TryGetProperty(
                        "serviceCode",
                        out var serviceCode) &&
                    serviceCode.ValueKind == JsonValueKind.String)
                {
                    return serviceCode.GetString();
                }

                if (root.TryGetProperty(
                        "service",
                        out var service) &&
                    service.ValueKind == JsonValueKind.String)
                {
                    return service.GetString();
                }
            }
            catch (JsonException)
            {
                // Historical malformed JSON falls through to other fields.
            }

            return null;
        }

        private static string? NormalizeSupportedVertical(
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

            return null;
        }
    }
}



//using Microsoft.EntityFrameworkCore;
//using MyApp.Api.Data;
//using MyApp.Api.Data.Entities;

//namespace MyApp.Api.Services.ExternalDeliveries
//{
//    public class ExternalLeadDistributionService
//    {
//        private readonly MyAppDbContext _db;
//        private readonly ILogger<ExternalLeadDistributionService> _logger;
//        public ExternalLeadDistributionService(
//            MyAppDbContext db,
//            ILogger<ExternalLeadDistributionService> logger)
//        {
//            _db = db;
//            _logger = logger;
//        }

//        // ============================================================
//        // NORMAL / PRODUCTION QUEUE
//        // ============================================================

//        public async Task QueueForLeadAsync(
//            long leadId,
//            CancellationToken cancellationToken = default)
//        {
//            var lead = await GetLeadAsync(
//                leadId,
//                cancellationToken);

//            if (lead == null)
//                return;

//            if (!lead.IsCompleted)
//            {
//                _logger.LogInformation(
//                    "External delivery skipped for LeadId {LeadId}: lead is incomplete.",
//                    leadId);

//                return;
//            }

//            /*
//             * Keep existing production protection.
//             *
//             * Test leads may pass because providers can use their
//             * configured test/staging environments.
//             */
//            if (!lead.IsTest &&
//                (
//                    lead.IsSuspicious ||
//                    lead.IsInvalidEmail ||
//                    lead.FraudScore >= 80
//                ))
//            {
//                _logger.LogInformation(
//                    "External delivery skipped for LeadId {LeadId}. " +
//                    "IsSuspicious={IsSuspicious}, " +
//                    "IsInvalidEmail={IsInvalidEmail}, " +
//                    "FraudScore={FraudScore}.",
//                    leadId,
//                    lead.IsSuspicious,
//                    lead.IsInvalidEmail,
//                    lead.FraudScore);

//                return;
//            }

//            var verticalCode =
//                ResolveVertical(lead);

//            if (verticalCode == null)
//            {
//                _logger.LogInformation(
//                    "External delivery skipped for LeadId {LeadId}: unsupported vertical.",
//                    leadId);

//                return;
//            }

//            await QueueAllApplicablePlatformsAsync(
//                lead.Id,
//                verticalCode,
//                cancellationToken);
//        }

//        // ============================================================
//        // MANUAL TEST QUEUE
//        //
//        // This is specifically for controlled integration testing.
//        // It DOES NOT weaken normal production fraud protection.
//        // ============================================================

//        public async Task QueueNewPlatformsForTestAsync(
//            long leadId,
//            CancellationToken cancellationToken = default)
//        {
//            var lead = await GetLeadAsync(
//                leadId,
//                cancellationToken);

//            if (lead == null)
//            {
//                throw new InvalidOperationException(
//                    $"Lead {leadId} was not found.");
//            }

//            if (!lead.IsCompleted)
//            {
//                throw new InvalidOperationException(
//                    $"Lead {leadId} is incomplete.");
//            }

//            var verticalCode =
//                ResolveVertical(lead);

//            if (verticalCode == null)
//            {
//                throw new InvalidOperationException(
//                    $"Unable to determine vertical for Lead {leadId}.");
//            }

//            /*
//             * CONTROLLED INTEGRATION TEST:
//             *
//             * Queue the existing providers directly for this one lead,
//             * bypassing only the normal fraud gate. Production
//             * QueueForLeadAsync remains unchanged.
//             *
//             * BlueInk        = applicable resolved verticals
//             * Networx        = applicable resolved verticals
//             * InsuranceTales = Windows only
//             * Modernize      = Roofing / Windows / HVAC / Bathroom
//             * Mili           = Roofing / Windows / HVAC / Bathroom
//             *
//             * Thumbtack is not an IExternalLeadProvider. Its business-search
//             * Request Flow is handled separately by ThumbtackApiService.
//             */

//            await QueuePlatformAsync(
//                lead.Id,
//                "BLUEINK",
//                verticalCode,
//                cancellationToken);

//            await QueuePlatformAsync(
//                lead.Id,
//                "NETWORX",
//                verticalCode,
//                cancellationToken);

//            if (string.Equals(
//                    verticalCode,
//                    "Windows",
//                    StringComparison.OrdinalIgnoreCase))
//            {
//                await QueuePlatformAsync(
//                    lead.Id,
//                    "InsuranceTales",
//                    verticalCode,
//                    cancellationToken);
//            }

//            if (IsHomeImprovementVertical(verticalCode))
//            {
//                await QueuePlatformAsync(
//                    lead.Id,
//                    "Modernize",
//                    verticalCode,
//                    cancellationToken);

//                await QueuePlatformAsync(
//                    lead.Id,
//                    "Mili",
//                    verticalCode,
//                    cancellationToken);
//            }

//            _logger.LogInformation(
//                "Manual external integration test queued. LeadId={LeadId}, Vertical={Vertical}.",
//                lead.Id,
//                verticalCode);
//        }

//        // ============================================================
//        // NORMAL PLATFORM DISTRIBUTION
//        // ============================================================

//        private async Task QueueAllApplicablePlatformsAsync(
//            long leadId,
//            string verticalCode,
//            CancellationToken cancellationToken)
//        {
//            /*
//             * Existing platforms.
//             * Keep these exactly as existing behavior.
//             */

//            await QueuePlatformAsync(
//                leadId,
//                "BLUEINK",
//                verticalCode,
//                cancellationToken);

//            await QueuePlatformAsync(
//                leadId,
//                "NETWORX",
//                verticalCode,
//                cancellationToken);

//            /*
//             * InsuranceTales / Alpha Living
//             * Windows campaign only.
//             */

//            if (string.Equals(
//                    verticalCode,
//                    "Windows",
//                    StringComparison.OrdinalIgnoreCase))
//            {
//                await QueuePlatformAsync(
//                    leadId,
//                    "InsuranceTales",
//                    verticalCode,
//                    cancellationToken);
//            }

//            /*
//             * Modernize
//             * Current supported Homeyy integrations:
//             * Roofing, Windows, HVAC, Bathroom
//             */

//            if (IsHomeImprovementVertical(verticalCode))
//            {
//                await QueuePlatformAsync(
//                    leadId,
//                    "Modernize",
//                    verticalCode,
//                    cancellationToken);
//            }

//            /*
//             * MILI
//             * Roofing, Windows, HVAC, Bathroom
//             */

//            if (IsHomeImprovementVertical(verticalCode))
//            {
//                await QueuePlatformAsync(
//                    leadId,
//                    "Mili",
//                    verticalCode,
//                    cancellationToken);
//            }
//        }

//        // ============================================================
//        // CREATE DELIVERY QUEUE ROW
//        // ============================================================
//        private async Task QueuePlatformAsync(
//            long leadId,
//            string platformCode,
//            string verticalCode,
//            CancellationToken cancellationToken)
//        {
//            var alreadyExists =
//                await _db.ExternalLeadDeliveries
//                    .AnyAsync(
//                        x =>
//                            x.LeadId == leadId &&
//                            x.PlatformCode == platformCode,
//                        cancellationToken);

//            if (alreadyExists)
//            {
//                _logger.LogInformation(
//                    "External delivery already exists. LeadId={LeadId}, Platform={Platform}.",
//                    leadId,
//                    platformCode);

//                return;
//            }

//            var now =
//                DateTime.UtcNow;

//            var delivery =
//                new ExternalLeadDelivery
//                {
//                    LeadId =
//                        leadId,

//                    PlatformCode =
//                        platformCode,

//                    VerticalCode =
//                        verticalCode,

//                    Status =
//                        "Pending",

//                    AttemptCount =
//                        0,

//                    MaxAttempts =
//                        5,

//                    NextAttemptOn =
//                        now,

//                    CreatedOn =
//                        now,

//                    UpdatedOn =
//                        now
//                };

//            _db.ExternalLeadDeliveries.Add(
//                delivery);

//            try
//            {
//                await _db.SaveChangesAsync(
//                    cancellationToken);

//                _logger.LogInformation(
//                    "External delivery queued. LeadId={LeadId}, Platform={Platform}, Vertical={Vertical}.",
//                    leadId,
//                    platformCode,
//                    verticalCode);
//            }
//            catch (DbUpdateException)
//            {
//                /*
//                 * Handles concurrent attempts when a unique index
//                 * already protects LeadId + PlatformCode.
//                 */

//                var existsNow =
//                    await _db.ExternalLeadDeliveries
//                        .AnyAsync(
//                            x =>
//                                x.LeadId == leadId &&
//                                x.PlatformCode == platformCode,
//                            cancellationToken);

//                if (!existsNow)
//                    throw;
//            }
//        }

//        // ============================================================
//        // LOAD LEAD
//        // ============================================================

//        private async Task<Lead?> GetLeadAsync(
//            long leadId,
//            CancellationToken cancellationToken)
//        {
//            return await _db.Leads
//                .Include(x => x.LeadType)
//                .FirstOrDefaultAsync(
//                    x =>
//                        x.Id == leadId &&
//                        !x.IsDeleted,
//                    cancellationToken);
//        }

//        // ============================================================
//        // SUPPORTED NEW PLATFORM VERTICALS
//        // ============================================================

//        private static bool IsHomeImprovementVertical(
//            string verticalCode)
//        {
//            return
//                string.Equals(
//                    verticalCode,
//                    "Roofing",
//                    StringComparison.OrdinalIgnoreCase)
//                ||
//                string.Equals(
//                    verticalCode,
//                    "Windows",
//                    StringComparison.OrdinalIgnoreCase)
//                ||
//                string.Equals(
//                    verticalCode,
//                    "HVAC",
//                    StringComparison.OrdinalIgnoreCase)
//                ||
//                string.Equals(
//                    verticalCode,
//                    "Bathroom",
//                    StringComparison.OrdinalIgnoreCase);
//        }

//        // ============================================================
//        // RESOLVE HOMEYY VERTICAL
//        // ============================================================

//        private static string? ResolveVertical(
//            Lead lead)
//        {
//            /*
//             * Prefer LeadTypeId because it is deterministic.
//             *
//             * Existing Homeyy mapping:
//             *
//             * 1 = Roofing
//             * 2 = Windows
//             * 4 = HVAC
//             * 5 = Bathroom
//             */

//            switch (lead.LeadTypeId)
//            {
//                case 1:
//                    return "Roofing";

//                case 2:
//                    return "Windows";

//                case 4:
//                    return "HVAC";

//                case 5:
//                    return "Bathroom";
//            }

//            /*
//             * Fallback for historical leads where LeadTypeId
//             * may not have been populated.
//             */

//            var possibleValues =
//                new[]
//                {
//                    lead.LeadType?.Name,
//                    lead.CampaignName,
//                    lead.PageName
//                };

//            foreach (var item in possibleValues)
//            {
//                if (string.IsNullOrWhiteSpace(item))
//                    continue;

//                var normalized =
//                    item
//                        .Trim()
//                        .Replace(" ", string.Empty)
//                        .Replace("-", string.Empty)
//                        .Replace("_", string.Empty)
//                        .ToLowerInvariant();

//                if (normalized.Contains("roof"))
//                    return "Roofing";

//                if (normalized.Contains("window"))
//                    return "Windows";

//                if (normalized.Contains("bath"))
//                    return "Bathroom";

//                if (
//                    normalized.Contains("hvac") ||
//                    normalized.Contains("heating") ||
//                    normalized.Contains("cooling") ||
//                    normalized.Contains("furnace") ||
//                    normalized.Contains("airconditioning")
//                )
//                {
//                    return "HVAC";
//                }
//            }

//            return null;
//        }
//    }
//}


