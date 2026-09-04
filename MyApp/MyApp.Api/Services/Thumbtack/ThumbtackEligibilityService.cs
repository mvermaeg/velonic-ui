using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Thumbtack
{
    public sealed class ThumbtackEligibilityService
    {
        private readonly MyAppDbContext _db;
        private readonly ThumbtackOptions _options;

        private static readonly string[] ForbiddenMarketingTerms =
        {
            "free",
            "government",
            "grant",
            "grants",
            "veteran",
            "veterans",
            "senior benefits",
            "qualify",
            "program"
        };

        public ThumbtackEligibilityService(
            MyAppDbContext db,
            IOptions<ThumbtackOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        public async Task<(bool Allowed, string? Reason)>
            CheckAsync(
                Lead lead,
                string categoryCode,
                CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return (false, "Thumbtack is disabled.");

            if (!IsSupportedCategory(categoryCode))
                return (false,
                    $"Thumbtack category {categoryCode} is not enabled.");

            var text =
                string.Join(
                    " ",
                    lead.CampaignName,
                    lead.PageName,
                    lead.AdditionalDataJson)
                .ToLowerInvariant();

            if (ForbiddenMarketingTerms.Any(text.Contains))
            {
                return (
                    false,
                    "Lead contains marketing language prohibited by Thumbtack.");
            }

            var startOfWeekUtc =
                GetStartOfWeekUtc();

            var sentThisWeek =
                await _db.ThumbtackSessions
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CategoryCode == categoryCode &&
                            x.CreatedOn >= startOfWeekUtc &&
                            x.Status != "Rejected",
                        cancellationToken);

            if (sentThisWeek >=
                _options.WeeklyCapPerCategory)
            {
                return (
                    false,
                    $"Thumbtack weekly cap reached for {categoryCode}.");
            }

            return (true, null);
        }

        private bool IsSupportedCategory(
            string categoryCode)
        {
            return categoryCode switch
            {
                "Roofing" => _options.Roofing.Enabled,
                "Windows" => _options.Windows.Enabled,
                "Bathroom" => _options.Bathroom.Enabled,
                "Gutters" => _options.Gutters.Enabled,
                _ => false
            };
        }

        private static DateTime GetStartOfWeekUtc()
        {
            var now = DateTime.UtcNow;

            var diff =
                (7 +
                 (now.DayOfWeek - DayOfWeek.Monday))
                % 7;

            return now.Date.AddDays(-diff);
        }
    }
}