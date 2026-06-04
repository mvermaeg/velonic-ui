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

        //public async Task RunForLeadAsync(long leadId)
        //{
        //    var lead = await _db.Leads
        //        .FirstOrDefaultAsync(x => x.Id == leadId && x.IsDeleted == false);

        //    if (lead == null)
        //        return;

        //    var existingResults = await _db.LeadBiddingResults
        //        .AnyAsync(x => x.LeadId == leadId);

        //    if (existingResults)
        //        return;

        //    var query =
        //        from setting in _db.ClientBiddingSettings
        //        join client in _db.Clients on setting.ClientId equals client.Id
        //        where setting.IsActive == true
        //              && client.IsDeleted == false
        //              && client.AccountStatus == "Active"
        //              && client.IsBuyer == true
        //        select new
        //        {
        //            Setting = setting,
        //            Client = client
        //        };

        //    if (!string.IsNullOrWhiteSpace(lead.State))
        //    {
        //        query = query.Where(x =>
        //            x.Setting.State == null ||
        //            x.Setting.State == "" ||
        //            x.Setting.State == lead.State);
        //    }

        //    if (!string.IsNullOrWhiteSpace(lead.Postcode))
        //    {
        //        query = query.Where(x =>
        //            x.Setting.Postcode == null ||
        //            x.Setting.Postcode == "" ||
        //            x.Setting.Postcode == lead.Postcode);
        //    }

        //    var matchedSettings = await query
        //        .OrderByDescending(x => x.Setting.BidAmount)
        //        .Take(10)
        //        .ToListAsync();

        //    if (!matchedSettings.Any())
        //        return;

        //    var highestBid = matchedSettings.Max(x => x.Setting.BidAmount);

        //    foreach (var item in matchedSettings)
        //    {
        //        var result = new LeadBiddingResult
        //        {
        //            LeadId = lead.Id,
        //            ClientId = item.Client.Id,
        //            ClientBiddingSettingId = item.Setting.Id,
        //            BidAmount = item.Setting.BidAmount,
        //            MatchReason = BuildMatchReason(lead, item.Setting),
        //            IsWon = item.Setting.BidAmount == highestBid,
        //            IsSold = false,
        //            SoldOn = null,
        //            CreatedOn = DateTime.UtcNow
        //        };

        //        _db.LeadBiddingResults.Add(result);
        //    }

        //    await _db.SaveChangesAsync();
        //}
        public async Task RunForLeadAsync(long leadId)
        {
            var lead = await _db.Leads
                .FirstOrDefaultAsync(x => x.Id == leadId && !x.IsDeleted);

            if (lead == null)
                return;

            var leadType = !string.IsNullOrWhiteSpace(lead.CampaignName)
                ? lead.CampaignName.Trim()
                : null;

            if (string.IsNullOrWhiteSpace(leadType))
                return;

            var postcode = lead.Postcode?.Trim();
            var state = lead.State?.Trim();

            var todayStart = DateTime.UtcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var matchedSettings = await _db.ClientBiddingSettings
                .Where(x =>
                    x.IsActive &&
                    x.LeadType == leadType &&
                    (string.IsNullOrWhiteSpace(x.Postcode) || x.Postcode == postcode) &&
                    (string.IsNullOrWhiteSpace(x.State) || x.State == state))
                .OrderByDescending(x => x.BidAmount)
                .ThenBy(x => x.Id)
                .ToListAsync();

            if (!matchedSettings.Any())
                return;

            ClientBiddingSetting? winner = null;

            foreach (var setting in matchedSettings)
            {
                var todaySoldCount = await _db.LeadBiddingResults.CountAsync(x =>
                    x.ClientBiddingSettingId == setting.Id &&
                    x.IsWon &&
                    x.IsSold &&
                    x.SoldOn >= todayStart &&
                    x.SoldOn < todayEnd);

                var monthSoldCount = await _db.LeadBiddingResults.CountAsync(x =>
                    x.ClientBiddingSettingId == setting.Id &&
                    x.IsWon &&
                    x.IsSold &&
                    x.SoldOn >= monthStart &&
                    x.SoldOn < monthEnd);

                var dailyCapReached =
                    setting.DailyCap.HasValue &&
                    setting.DailyCap.Value > 0 &&
                    todaySoldCount >= setting.DailyCap.Value;

                var monthlyCapReached =
                    setting.MonthlyCap.HasValue &&
                    setting.MonthlyCap.Value > 0 &&
                    monthSoldCount >= setting.MonthlyCap.Value;

                if (dailyCapReached || monthlyCapReached)
                    continue;

                winner = setting;
                break;
            }

            if (winner == null)
                return;

            var alreadyExists = await _db.LeadBiddingResults
                .AnyAsync(x => x.LeadId == lead.Id && x.ClientId == winner.ClientId);

            if (alreadyExists)
                return;

            var result = new LeadBiddingResult
            {
                LeadId = lead.Id,
                ClientId = winner.ClientId,
                ClientBiddingSettingId = winner.Id,
                BidAmount = winner.BidAmount,
                MatchReason =
                    $"Matched by LeadType: {winner.LeadType}, State: {winner.State}, Postcode: {winner.Postcode}, Bid: {winner.BidAmount}",
                IsWon = true,
                IsSold = false,
                SoldOn = null,
                CreatedOn = DateTime.UtcNow
            };

            _db.LeadBiddingResults.Add(result);
            await _db.SaveChangesAsync();
        }

        //public async Task RunForLeadAsync(long leadId)
        //{
        //    var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == leadId && !x.IsDeleted);

        //    if (lead == null)
        //        return;

        //    if (string.IsNullOrWhiteSpace(lead.CampaignName))
        //        return;

        //    var leadType = lead.CampaignName.Trim();
        //    var postcode = lead.Postcode?.Trim();
        //    var state = lead.State?.Trim();

        //    var todayStart = DateTime.UtcNow.Date;
        //    var todayEnd = todayStart.AddDays(1);

        //    var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        //    var monthEnd = monthStart.AddMonths(1);

        //    var matchingSettings = await _db.ClientBiddingSettings
        //        .Where(x =>
        //            x.IsActive &&
        //            x.LeadType == leadType &&
        //            (string.IsNullOrWhiteSpace(x.Postcode) || x.Postcode == postcode) &&
        //            (string.IsNullOrWhiteSpace(x.State) || x.State == state))
        //        .OrderByDescending(x => x.BidAmount)
        //        .ToListAsync();

        //    if (!matchingSettings.Any())
        //        return;

        //    var eligibleSettings = new List<ClientBiddingSetting>();

        //    foreach (var setting in matchingSettings)
        //    {
        //        var todaySoldCount = await _db.LeadBiddingResults.CountAsync(x =>
        //            x.ClientId == setting.ClientId &&
        //            x.IsWon &&
        //            x.IsSold &&
        //            x.SoldOn >= todayStart &&
        //            x.SoldOn < todayEnd);

        //        var monthSoldCount = await _db.LeadBiddingResults.CountAsync(x =>
        //            x.ClientId == setting.ClientId &&
        //            x.IsWon &&
        //            x.IsSold &&
        //            x.SoldOn >= monthStart &&
        //            x.SoldOn < monthEnd);

        //        var dailyCapReached =
        //            setting.DailyCap.HasValue &&
        //            setting.DailyCap.Value > 0 &&
        //            todaySoldCount >= setting.DailyCap.Value;

        //        var monthlyCapReached =
        //            setting.MonthlyCap.HasValue &&
        //            setting.MonthlyCap.Value > 0 &&
        //            monthSoldCount >= setting.MonthlyCap.Value;

        //        if (dailyCapReached || monthlyCapReached)
        //            continue;

        //        eligibleSettings.Add(setting);
        //    }

        //    if (!eligibleSettings.Any())
        //        return;

        //    var winner = eligibleSettings
        //        .OrderByDescending(x => x.BidAmount)
        //        .First();

        //    var result = new LeadBiddingResult
        //    {
        //        LeadId = lead.Id,
        //        ClientId = winner.ClientId,
        //        ClientBiddingSettingId = winner.Id,
        //        BidAmount = winner.BidAmount,
        //        MatchReason = $"Matched by LeadType: {winner.LeadType}, Postcode: {winner.Postcode}, Bid: {winner.BidAmount}",
        //        IsWon = true,
        //        IsSold = false,
        //        CreatedOn = DateTime.UtcNow
        //    };

        //    _db.LeadBiddingResults.Add(result);
        //    await _db.SaveChangesAsync();
        //}

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