using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;

namespace MyApp.Api.Services.Deliveries
{
    public class LeadDeliveryService
    {
        private readonly MyAppDbContext _db;

        public LeadDeliveryService(MyAppDbContext db)
        {
            _db = db;
        }

        public async Task<long> CreateDeliveryFromBiddingResultAsync(long biddingResultId)
        {
            var biddingResult = await _db.LeadBiddingResults
                .FirstOrDefaultAsync(x => x.Id == biddingResultId);

            if (biddingResult == null)
                throw new Exception("Bidding result not found.");

            if (biddingResult.IsSold)
                throw new Exception("This lead is already sold.");

            var delivery = new LeadDelivery
            {
                LeadId = biddingResult.LeadId,
                ClientId = biddingResult.ClientId,
                LeadBiddingResultId = biddingResult.Id,
                DeliveryType = "Manual",
                DeliveryStatus = "Delivered",
                DeliveredOn = DateTime.UtcNow,
                ResponseMessage = "Lead manually delivered from bidding result.",
                CreatedOn = DateTime.UtcNow
            };

            _db.LeadDeliveries.Add(delivery);

            biddingResult.IsSold = true;
            biddingResult.SoldOn = DateTime.UtcNow;
            biddingResult.IsWon = true;

            var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == biddingResult.LeadId);
            if (lead != null)
            {
                lead.LeadStatus = "Sold";
                lead.UpdatedOn = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return delivery.Id;
        }
    }
}