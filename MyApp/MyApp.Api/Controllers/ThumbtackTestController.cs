using Microsoft.AspNetCore.Mvc;
using MyApp.Api.Services.ExternalDeliveries.Thumbtack;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ThumbtackTestController : ControllerBase
    {
        private readonly ThumbtackApiService _thumbtackApiService;
        private readonly ThumbtackOptions _options;

        public ThumbtackTestController(
            ThumbtackApiService thumbtackApiService,
            Microsoft.Extensions.Options.IOptions<ThumbtackOptions> options)
        {
            _thumbtackApiService = thumbtackApiService;
            _options = options.Value;
        }

        [HttpGet("roofing")]
        public async Task<IActionResult> TestRoofing(
            [FromQuery] string zipCode = "90210",
            CancellationToken cancellationToken = default)
        {
            try
            {
                var categoryId =
                    _options.Roofing.CategoryPk;

                var trackingId =
                    $"HOMEYY-TEST-{Guid.NewGuid():N}";

                var result =
                    await _thumbtackApiService.SearchBusinessesAsync(
                        categoryId,
                        zipCode,
                        trackingId,
                        10,
                        cancellationToken);

                return Ok(new
                {
                    success = true,
                    environment =
                        _options.UseStaging
                            ? "Staging"
                            : "Production",

                    categoryId,
                    zipCode,
                    trackingId,

                    searchId =
                        result.SearchId,

                    count =
                        result.Data.Count,

                    businesses =
                        result.Data.Select(x => new
                        {
                            x.BusinessId,
                            x.BusinessName,
                            x.BusinessLocation,
                            x.Rating,
                            x.NumberOfReviews,
                            x.IsTopPro,

                            requestFlowUrl =
                                x.Widgets?.RequestFlowUrl,

                            servicePageUrl =
                                x.Widgets?.ServicePageUrl
                        }),

                    metadata =
                        result.Metadata
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error =
                        ex.Message,

                    innerError =
                        ex.InnerException?.Message
                });
            }
        }
    }
}