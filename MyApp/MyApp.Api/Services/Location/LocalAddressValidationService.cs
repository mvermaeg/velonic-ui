using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.DTOs.Location;

namespace MyApp.Api.Services.Location
{
    public class LocalAddressValidationService : IAddressValidationService
    {
        private readonly MyAppDbContext _db;

        public LocalAddressValidationService(MyAppDbContext db)
        {
            _db = db;
        }

        public async Task<VerifyAddressPostcodeResponse> VerifyAsync(VerifyAddressPostcodeRequest model)
        {
            var address = model.Address?.Trim().ToLower() ?? "";
            var postcode = model.Postcode?.Trim().ToUpper() ?? "";

            if (string.IsNullOrWhiteSpace(address) || string.IsNullOrWhiteSpace(postcode))
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "Mismatch",
                    Message = "Address and postcode are required."
                };
            }

            var record = await _db.PostalCodeLocations
                .Where(x => x.IsActive && x.PostalCode.ToUpper() == postcode)
                .FirstOrDefaultAsync();

            if (record == null)
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "Mismatch",
                    Message = "Postcode not found in our service area."
                };
            }

            var city = record.City?.Trim() ?? "";
            var stateName = record.StateName?.Trim() ?? "";
            var stateCode = record.StateCode?.Trim() ?? "";

            var cityMatch = !string.IsNullOrWhiteSpace(city) &&
                            address.Contains(city.ToLower());

            var stateNameMatch = !string.IsNullOrWhiteSpace(stateName) &&
                                 address.Contains(stateName.ToLower());

            var stateCodeMatch = !string.IsNullOrWhiteSpace(stateCode) &&
                                 address.Contains(stateCode.ToLower());

            var accepted = cityMatch || stateNameMatch || stateCodeMatch;

            if (!accepted)
            {
                return new VerifyAddressPostcodeResponse
                {
                    IsValid = false,
                    Status = "Mismatch",
                    Message = $"Address does not match postcode. Expected location: {city}, {stateName}.",
                    PostalCode = record.PostalCode,
                    City = city,
                    State = stateName,
                    Country = record.Country
                };
            }

            return new VerifyAddressPostcodeResponse
            {
                IsValid = true,
                Status = "Valid",
                Message = "Address and postcode verified.",
                PostalCode = record.PostalCode,
                City = city,
                State = stateName,
                Country = record.Country,
                PossibleNextAction = "ACCEPT"
            };
        }
    }
}