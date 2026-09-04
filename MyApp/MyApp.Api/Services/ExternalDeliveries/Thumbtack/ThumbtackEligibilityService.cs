using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.ExternalDeliveries.Thumbtack
{
    public sealed class ThumbtackEligibilityService
    {
        private readonly MyAppDbContext _db;
        private readonly ThumbtackOptions _options;

        private static readonly string[] ForbiddenTerms =
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

        public async Task<(bool Allowed, string? Reason)> CheckAsync(
            Lead lead,
            string verticalCode,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return (false, "Thumbtack integration is disabled.");

            var vertical = GetVerticalOptions(verticalCode);

            if (vertical == null || !vertical.Enabled)
                return (false, $"Thumbtack vertical {verticalCode} is disabled.");

            var text = string.Join(" ",
                lead.CampaignName,
                lead.PageName,
                lead.AdditionalDataJson
            ).ToLowerInvariant();

            foreach (var term in ForbiddenTerms)
            {
                if (text.Contains(term))
                {
                    return (
                        false,
                        $"Thumbtack blocked due to prohibited marketing term: {term}"
                    );
                }
            }

            var startOfWeek = GetStartOfWeekUtc();

            var count = await _db.ExternalLeadDeliveries
                .AsNoTracking()
                .CountAsync(
                    x =>
                        x.PlatformCode == "THUMBTACK" &&
                        x.VerticalCode == verticalCode &&
                        x.CreatedOn >= startOfWeek,
                    cancellationToken);

            if (count >= _options.WeeklyCapPerVertical)
            {
                return (
                    false,
                    $"Thumbtack weekly cap reached for {verticalCode}."
                );
            }

            return (true, null);
        }

        private ThumbtackVerticalOptions? GetVerticalOptions(
            string verticalCode)
        {
            return verticalCode switch
            {
                "Roofing" => _options.Roofing,
                "Windows" => _options.Windows,
                "Bathroom" => _options.Bathroom,
                "Gutters" => _options.Gutters,
                _ => null
            };
        }

        private static DateTime GetStartOfWeekUtc()
        {
            var now = DateTime.UtcNow;

            var diff =
                (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;

            return now.Date.AddDays(-diff);
        }
    }
}