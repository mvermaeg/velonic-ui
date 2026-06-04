using MyApp.Api.DTOs.Location;

namespace MyApp.Api.Services.Location
{
    public interface IAddressValidationService
    {
        Task<VerifyAddressPostcodeResponse> VerifyAsync(VerifyAddressPostcodeRequest model);
    }
}