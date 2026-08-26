using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

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

        public async Task QueueForLeadAsync(
            long leadId,
            CancellationToken cancellationToken = default)
        {
            var lead = await _db.Leads
                .Include(x => x.LeadType)
                .FirstOrDefaultAsync(
                    x => x.Id == leadId && !x.IsDeleted,
                    cancellationToken);

            if (lead == null)
                return;

            /*
             * Do not distribute blocked/fraudulent/test/incomplete leads.
             * Adjust the FraudScore threshold later if required.
             */
            if (!lead.IsCompleted)
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}: lead is incomplete.",
                    leadId);

                return;
            }

            // Test leads may continue to the provider's test endpoint.
            // Live leads remain protected by fraud and email checks.
            if (!lead.IsTest &&
                (lead.IsSuspicious ||
                 lead.IsInvalidEmail ||
                 lead.FraudScore >= 80))
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}. " +
                    "IsSuspicious: {IsSuspicious}, IsInvalidEmail: {IsInvalidEmail}, FraudScore: {FraudScore}.",
                    leadId,
                    lead.IsSuspicious,
                    lead.IsInvalidEmail,
                    lead.FraudScore);

                return;
            }

            var verticalCode = ResolveVertical(lead);

            if (verticalCode == null)
            {
                _logger.LogInformation(
                    "External delivery skipped for LeadId {LeadId}: unsupported vertical.",
                    leadId);

                return;
            }

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


            /*
             * Add this after NetworxLeadProvider is created:
             *
             * await QueuePlatformAsync(
             *     lead.Id,
             *     "NETWORX",
             *     verticalCode,
             *     cancellationToken);
             */
        }

        private async Task QueuePlatformAsync(
            long leadId,
            string platformCode,
            string verticalCode,
            CancellationToken cancellationToken)
        {
            var alreadyExists = await _db.ExternalLeadDeliveries
                .AnyAsync(
                    x =>
                        x.LeadId == leadId &&
                        x.PlatformCode == platformCode,
                    cancellationToken);

            if (alreadyExists)
                return;

            var delivery = new ExternalLeadDelivery
            {
                LeadId = leadId,
                PlatformCode = platformCode,
                VerticalCode = verticalCode,
                Status = "Pending",
                AttemptCount = 0,
                MaxAttempts = 5,
                NextAttemptOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow
            };

            _db.ExternalLeadDeliveries.Add(delivery);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                /*
                 * The unique index protects against two requests queuing
                 * the same lead/platform concurrently.
                 */
                var existsNow = await _db.ExternalLeadDeliveries
                    .AnyAsync(
                        x =>
                            x.LeadId == leadId &&
                            x.PlatformCode == platformCode,
                        cancellationToken);

                if (!existsNow)
                    throw;
            }
        }

        private static string? ResolveVertical(Lead lead)
        {
            var possibleValues = new[]
            {
                lead.LeadType?.Name,
                lead.CampaignName,
                lead.PageName
            };

            foreach (var item in possibleValues)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                var normalized = item
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

                if (normalized.Contains("hvac") ||
                    normalized.Contains("heating") ||
                    normalized.Contains("cooling") ||
                    normalized.Contains("furnace") ||
                    normalized.Contains("airconditioning"))
                {
                    return "HVAC";
                }
            }

            return null;
        }
    }
}