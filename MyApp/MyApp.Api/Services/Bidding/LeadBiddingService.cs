using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Bidding
{
    public class LeadBiddingService
    {
        private readonly MyAppDbContext _db;

        public LeadBiddingService(MyAppDbContext db)
        {
            _db = db;
        }

        public async Task RunForLeadAsync(long leadId)
        {
            var lead = await _db.Leads
                .FirstOrDefaultAsync(x => x.Id == leadId && x.IsDeleted == false);

            if (lead == null)
                return;

            var existingResults = await _db.LeadBiddingResults
                .AnyAsync(x => x.LeadId == leadId);

            if (existingResults)
                return;

            var query =
                from setting in _db.ClientBiddingSettings
                join client in _db.Clients on setting.ClientId equals client.Id
                where setting.IsActive == true
                      && client.IsDeleted == false
                      && client.AccountStatus == "Active"
                      && client.IsBuyer == true
                select new
                {
                    Setting = setting,
                    Client = client
                };

            if (!string.IsNullOrWhiteSpace(lead.State))
            {
                query = query.Where(x =>
                    x.Setting.State == null ||
                    x.Setting.State == "" ||
                    x.Setting.State == lead.State);
            }

            if (!string.IsNullOrWhiteSpace(lead.Postcode))
            {
                query = query.Where(x =>
                    x.Setting.Postcode == null ||
                    x.Setting.Postcode == "" ||
                    x.Setting.Postcode == lead.Postcode);
            }

            var matchedSettings = await query
                .OrderByDescending(x => x.Setting.BidAmount)
                .Take(10)
                .ToListAsync();

            if (!matchedSettings.Any())
                return;

            var highestBid = matchedSettings.Max(x => x.Setting.BidAmount);

            foreach (var item in matchedSettings)
            {
                var result = new LeadBiddingResult
                {
                    LeadId = lead.Id,
                    ClientId = item.Client.Id,
                    ClientBiddingSettingId = item.Setting.Id,
                    BidAmount = item.Setting.BidAmount,
                    MatchReason = BuildMatchReason(lead, item.Setting),
                    IsWon = item.Setting.BidAmount == highestBid,
                    IsSold = false,
                    SoldOn = null,
                    CreatedOn = DateTime.UtcNow
                };

                _db.LeadBiddingResults.Add(result);
            }

            await _db.SaveChangesAsync();
        }

        private static string BuildMatchReason(Lead lead, ClientBiddingSetting setting)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(setting.LeadType))
                parts.Add($"LeadType: {setting.LeadType}");

            if (!string.IsNullOrWhiteSpace(setting.State))
                parts.Add($"State: {setting.State}");

            if (!string.IsNullOrWhiteSpace(setting.Postcode))
                parts.Add($"Postcode: {setting.Postcode}");

            if (!parts.Any())
                return "Matched by active client bidding rule.";

            return "Matched by " + string.Join(", ", parts);
        }
    }
}