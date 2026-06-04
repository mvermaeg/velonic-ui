using Microsoft.AspNetCore.Mvc;
using MyApp.Api.DTOs.Location;
using MyApp.Api.Services.Location;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/location")]
    public class LocationController : ControllerBase
    {
        private readonly GoogleAddressValidationService _addressValidationService;

        public LocationController(GoogleAddressValidationService addressValidationService)
        {
            _addressValidationService = addressValidationService;
        }

        [HttpPost("verify-address-postcode")]
        public async Task<IActionResult> VerifyAddressPostcode(
            [FromBody] VerifyAddressPostcodeRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.Address))
                return BadRequest(new { message = "Address is required." });

            if (string.IsNullOrWhiteSpace(model.Postcode))
                return BadRequest(new { message = "Postcode is required." });

            var result = await _addressValidationService.VerifyAsync(model);

            return Ok(result);
        }
    }
}