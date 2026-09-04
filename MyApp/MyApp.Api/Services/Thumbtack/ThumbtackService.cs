using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Thumbtack
{
    public sealed class ThumbtackService
    {
        private readonly MyAppDbContext _db;
        private readonly ThumbtackOptions _options;
        private readonly ThumbtackEligibilityService _eligibility;

        public ThumbtackService(
            MyAppDbContext db,
            IOptions<ThumbtackOptions> options,
            ThumbtackEligibilityService eligibility)
        {
            _db = db;
            _options = options.Value;
            _eligibility = eligibility;
        }

        public async Task<object> CreateSessionAsync(
            long leadId,
            CancellationToken cancellationToken = default)
        {
            var lead =
                await _db.Leads
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == leadId &&
                            !x.IsDeleted,
                        cancellationToken);

            if (lead == null)
            {
                throw new InvalidOperationException(
                    $"Lead {leadId} not found.");
            }

            var categoryCode =
                ResolveCategory(lead);

            if (categoryCode == null)
            {
                throw new InvalidOperationException(
                    "Lead is not currently supported by Thumbtack.");
            }

            var eligibility =
                await _eligibility.CheckAsync(
                    lead,
                    categoryCode,
                    cancellationToken);

            if (!eligibility.Allowed)
            {
                throw new InvalidOperationException(
                    eligibility.Reason);
            }

            var category =
                GetCategory(categoryCode);

            if (string.IsNullOrWhiteSpace(
                    category.CategoryPk))
            {
                throw new InvalidOperationException(
                    $"Thumbtack CategoryPk is missing for {categoryCode}.");
            }

            var trackingId =
                $"HOMEYY-TT-{lead.Id}-{Guid.NewGuid():N}"
                    .ToUpperInvariant();

            var utmContent =
                trackingId;

            var session =
                new ThumbtackSession
                {
                    LeadId = lead.Id,
                    TrackingId = trackingId,
                    UtmContent = utmContent,

                    CategoryCode = categoryCode,
                    CategoryPk = category.CategoryPk,

                    ZipCode = lead.Postcode,

                    Status = "Created",

                    ExpectedPayout =
                        category.ExpectedPayout,

                    CreatedOn =
                        DateTime.UtcNow,

                    UpdatedOn =
                        DateTime.UtcNow
                };

            _db.ThumbtackSessions.Add(session);

            await _db.SaveChangesAsync(
                cancellationToken);

            return new
            {
                sessionId = session.Id,
                session.TrackingId,
                session.UtmContent,
                session.CategoryCode,
                session.CategoryPk,
                session.ZipCode,
                session.ExpectedPayout,

                utmSource =
                    _options.UtmSource,

                utmMedium =
                    _options.UtmMedium
            };
        }

        private ThumbtackCategoryOptions GetCategory(
            string code)
        {
            return code switch
            {
                "Roofing" => _options.Roofing,
                "Windows" => _options.Windows,
                "Bathroom" => _options.Bathroom,
                "Gutters" => _options.Gutters,

                _ => throw new
                    InvalidOperationException(
                        $"Unsupported category {code}.")
            };
        }

        private static string?
            ResolveCategory(Lead lead)
        {
            var text =
                $"{lead.CampaignName} {lead.PageName}"
                    .ToLowerInvariant();

            if (text.Contains("roof"))
                return "Roofing";

            if (text.Contains("window"))
                return "Windows";

            if (text.Contains("bath"))
                return "Bathroom";

            if (text.Contains("gutter"))
                return "Gutters";

            return null;
        }
    }
}