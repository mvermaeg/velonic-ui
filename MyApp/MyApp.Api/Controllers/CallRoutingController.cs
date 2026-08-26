using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Api.DTOs.Ringba;
using MyApp.Api.Services.Ringba;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/call-routing")]
    [Authorize]
    public class CallRoutingController : ControllerBase
    {
        private readonly RingbaService _ringbaService;
        private readonly ILogger<CallRoutingController> _logger;

        public CallRoutingController(
            RingbaService ringbaService,
            ILogger<CallRoutingController> logger)
        {
            _ringbaService = ringbaService;
            _logger = logger;
        }

        [HttpPost("ringba/bid")]
        public async Task<IActionResult> RequestRingbaBid(
            [FromBody] RingbaBidRequestDto model,
            CancellationToken cancellationToken)
        {
            if (model == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Request body is required."
                });
            }

            var result = await _ringbaService.RequestBidAsync(
                model,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogInformation(
                    "Ringba returned no usable bid. Vertical: {Vertical}, " +
                    "HTTP: {HttpStatusCode}, Error: {Error}",
                    model.Vertical,
                    result.HttpStatusCode,
                    result.Error);

                return Ok(new
                {
                    success = false,
                    hasBid = false,
                    message = result.Error ?? "No buyer bid was available.",
                    providerHttpStatusCode = result.HttpStatusCode,
                    rawResponse = result.RawResponse
                });
            }

            return Ok(new
            {
                success = true,
                hasBid = true,

                result.BidId,
                result.BidAmount,

                result.ExpireInSeconds,
                result.BidExpireDateTime,
                result.BidExpireEpoch,

                result.PhoneNumber,
                result.PhoneNumberNoPlus,
                result.SipAddress,

                result.BidTermsJson,
                result.WarningsJson,

                result.HttpStatusCode
            });
        }
    }
}