using Microsoft.AspNetCore.Mvc;
using MyApp.Api.DTOs.Location;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.Location;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/diagnostics")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly BouncerEmailValidationService _emailValidation;
        private readonly IAddressValidationService _addressValidationService;

        public DiagnosticsController(
            BouncerEmailValidationService emailValidation,
            IAddressValidationService addressValidationService)
        {
            _emailValidation = emailValidation;
            _addressValidationService = addressValidationService;
        }

        [HttpGet("email")]
        public async Task<IActionResult> TestEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required." });

            var result = await _emailValidation.VerifyAsync(email);

            return Ok(new
            {
                email,
                result
            });
        }

        [HttpPost("address")]
        public async Task<IActionResult> TestAddress([FromBody] VerifyAddressPostcodeRequest model)
        {
            var result = await _addressValidationService.VerifyAsync(model);

            return Ok(new
            {
                request = model,
                result
            });
        }
    }
}