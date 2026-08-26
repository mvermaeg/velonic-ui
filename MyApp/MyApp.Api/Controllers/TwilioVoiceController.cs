using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyApp.Api.DTOs.Ringba;
using MyApp.Api.Services.Ringba;
using MyApp.Api.Services.TwilioVoice;
using Twilio.Security;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/twilio/voice")]
    [AllowAnonymous]
    public class TwilioVoiceController : ControllerBase
    {
        private readonly RingbaService _ringbaService;
        private readonly TwilioVoiceOptions _options;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioVoiceController> _logger;

        public TwilioVoiceController(
            RingbaService ringbaService,
            IOptions<TwilioVoiceOptions> options,
            IConfiguration configuration,
            ILogger<TwilioVoiceController> logger)
        {
            _ringbaService = ringbaService;
            _options = options.Value;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Twilio calls this endpoint when a phone call reaches the Twilio number.
        /// </summary>
        [HttpPost("incoming")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Incoming()
        {
            if (!await IsValidTwilioRequestAsync())
            {
                return Unauthorized();
            }

            if (!_options.Enabled)
            {
                return Twiml(BuildDisabledResponse());
            }

            if (!_options.EnableRingbaRouting)
            {
                return Twiml(BuildFallbackResponse(
                    "Call routing is currently unavailable."));
            }

            var actionUrl = BuildWebhookUrl(
                "/api/twilio/voice/select-service");

            var xml = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <Response>
                    <Gather input="dtmf"
                            numDigits="1"
                            timeout="8"
                            action="{Xml(actionUrl)}"
                            method="POST">
                        <Say voice="{Xml(_options.Voice)}">
                            Press 1 for roofing.
                            Press 2 for windows.
                            Press 3 for heating and air conditioning.
                            Press 4 for plumbing.
                        </Say>
                    </Gather>
                    <Say voice="{Xml(_options.Voice)}">
                        We did not receive your selection.
                    </Say>
                    <Redirect method="POST">{Xml(BuildWebhookUrl("/api/twilio/voice/incoming"))}</Redirect>
                </Response>
                """;

            return Twiml(xml);
        }

        /// <summary>
        /// Receives the selected service and asks the caller for a ZIP code.
        /// </summary>
        [HttpPost("select-service")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> SelectService(
            [FromForm] string? Digits)
        {
            if (!await IsValidTwilioRequestAsync())
            {
                return Unauthorized();
            }

            if (!_options.Enabled ||
                !_options.EnableRingbaRouting)
            {
                return Twiml(BuildFallbackResponse(
                    "Call routing is currently unavailable."));
            }

            var vertical = Digits?.Trim() switch
            {
                "1" => "Roofing",
                "2" => "Windows",
                "3" => "HVAC",
                "4" => "Plumbing",
                _ => null
            };

            if (vertical == null)
            {
                var xml = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Response>
                        <Say voice="{Xml(_options.Voice)}">
                            That selection was not valid.
                        </Say>
                        <Redirect method="POST">{Xml(BuildWebhookUrl("/api/twilio/voice/incoming"))}</Redirect>
                    </Response>
                    """;

                return Twiml(xml);
            }

            var actionUrl = BuildWebhookUrl(
                $"/api/twilio/voice/route?vertical={Uri.EscapeDataString(vertical)}");

            var responseXml = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <Response>
                    <Gather input="dtmf"
                            numDigits="5"
                            timeout="10"
                            action="{Xml(actionUrl)}"
                            method="POST">
                        <Say voice="{Xml(_options.Voice)}">
                            Please enter your five digit ZIP code.
                        </Say>
                    </Gather>
                    <Say voice="{Xml(_options.Voice)}">
                        We did not receive a ZIP code.
                    </Say>
                    <Redirect method="POST">{Xml(BuildWebhookUrl("/api/twilio/voice/incoming"))}</Redirect>
                </Response>
                """;

            return Twiml(responseXml);
        }

        /// <summary>
        /// Requests the Ringba bid and immediately returns TwiML to transfer the call.
        /// </summary>
        [HttpPost("route")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Route(
            [FromQuery] string vertical,
            [FromForm] string? Digits,
            [FromForm] string? From,
            [FromForm] string? To,
            [FromForm] string? CallSid,
            CancellationToken cancellationToken)
        {
            if (!await IsValidTwilioRequestAsync())
            {
                return Unauthorized();
            }

            if (!_options.Enabled ||
                !_options.EnableRingbaRouting)
            {
                return Twiml(BuildFallbackResponse(
                    "Call routing is currently unavailable."));
            }

            var zipcode = new string(
                (Digits ?? string.Empty)
                    .Where(char.IsDigit)
                    .ToArray());

            if (zipcode.Length != 5)
            {
                var xml = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Response>
                        <Say voice="{Xml(_options.Voice)}">
                            The ZIP code entered was not valid.
                        </Say>
                        <Redirect method="POST">{Xml(BuildWebhookUrl("/api/twilio/voice/incoming"))}</Redirect>
                    </Response>
                    """;

                return Twiml(xml);
            }

            if (string.IsNullOrWhiteSpace(From))
            {
                return Twiml(BuildFallbackResponse(
                    "We could not identify the caller."));
            }

            _logger.LogInformation(
                "Requesting Ringba bid. CallSid: {CallSid}, Vertical: {Vertical}, ZIP: {Zipcode}",
                CallSid,
                vertical,
                zipcode);

            var bid = await _ringbaService.RequestBidAsync(
                new RingbaBidRequestDto
                {
                    Vertical = vertical,
                    CallerPhone = From,
                    ZipCode = zipcode,
                    SubId = _options.SubId
                },
                cancellationToken);

            if (!bid.Success)
            {
                _logger.LogInformation(
                    "Ringba returned no usable bid. CallSid: {CallSid}, Vertical: {Vertical}, Error: {Error}",
                    CallSid,
                    vertical,
                    bid.Error);

                return Twiml(BuildFallbackResponse(
                    "No service provider is currently available."));
            }

            _logger.LogInformation(
                "Ringba bid accepted. CallSid: {CallSid}, BidId: {BidId}, Amount: {BidAmount}, Expiry: {Expiry}",
                CallSid,
                bid.BidId,
                bid.BidAmount,
                bid.ExpireInSeconds);

            // Prefer SIP when Ringba provides it.
            if (!string.IsNullOrWhiteSpace(bid.SipAddress))
            {
                var sipAddress = bid.SipAddress.StartsWith(
                    "sip:",
                    StringComparison.OrdinalIgnoreCase)
                        ? bid.SipAddress
                        : $"sip:{bid.SipAddress}";

                var sipXml = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Response>
                        <Say voice="{Xml(_options.Voice)}">
                            Please hold while we connect your call.
                        </Say>
                        <Dial timeout="20"
                              answerOnBridge="true">
                            <Sip>{Xml(sipAddress)}</Sip>
                        </Dial>
                        <Say voice="{Xml(_options.Voice)}">
                            We were unable to complete the connection.
                        </Say>
                    </Response>
                    """;

                return Twiml(sipXml);
            }

            var destination =
                bid.PhoneNumber ??
                bid.PhoneNumberNoPlus;

            if (!string.IsNullOrWhiteSpace(destination))
            {
                if (!destination.StartsWith("+"))
                {
                    destination = "+" + destination;
                }

                var callerIdAttribute =
                    string.IsNullOrWhiteSpace(To)
                        ? string.Empty
                        : $" callerId=\"{Xml(To)}\"";

                var phoneXml = $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Response>
                        <Say voice="{Xml(_options.Voice)}">
                            Please hold while we connect your call.
                        </Say>
                        <Dial timeout="20"
                              answerOnBridge="true"{callerIdAttribute}>
                            <Number>{Xml(destination)}</Number>
                        </Dial>
                        <Say voice="{Xml(_options.Voice)}">
                            We were unable to complete the connection.
                        </Say>
                    </Response>
                    """;

                return Twiml(phoneXml);
            }

            return Twiml(BuildFallbackResponse(
                "No call destination was returned."));
        }

        private string BuildDisabledResponse()
        {
            return BuildFallbackResponse(
                "This call routing service is currently inactive.");
        }

        private string BuildFallbackResponse(
            string message)
        {
            if (!string.IsNullOrWhiteSpace(
                    _options.FallbackNumber))
            {
                var destination =
                    _options.FallbackNumber.Trim();

                return $"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <Response>
                        <Say voice="{Xml(_options.Voice)}">
                            {Xml(message)} Please hold while we transfer your call.
                        </Say>
                        <Dial timeout="20"
                              answerOnBridge="true">
                            <Number>{Xml(destination)}</Number>
                        </Dial>
                        <Say voice="{Xml(_options.Voice)}">
                            We are unable to complete your call at this time.
                        </Say>
                    </Response>
                    """;
            }

            return $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <Response>
                    <Say voice="{Xml(_options.Voice)}">
                        {Xml(message)} Please try again later.
                    </Say>
                    <Hangup />
                </Response>
                """;
        }

        private async Task<bool> IsValidTwilioRequestAsync()
        {
            if (!_options.ValidateTwilioSignature)
            {
                return true;
            }

            var authToken =
                _configuration["Twilio:AuthToken"];

            if (string.IsNullOrWhiteSpace(authToken))
            {
                _logger.LogError(
                    "Twilio signature validation is enabled, but Twilio:AuthToken is missing.");

                return false;
            }

            var signature =
                Request.Headers["X-Twilio-Signature"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            var form = await Request.ReadFormAsync();

            var parameters = form.ToDictionary(
                x => x.Key,
                x => x.Value.ToString());

            var requestUrl =
                GetExactPublicRequestUrl();

            var validator =
                new RequestValidator(authToken);

            return validator.Validate(
                requestUrl,
                parameters,
                signature);
        }

        private string GetExactPublicRequestUrl()
        {
            var pathAndQuery =
                Request.Path +
                Request.QueryString;

            if (!string.IsNullOrWhiteSpace(
                    _options.WebhookBaseUrl))
            {
                return
                    _options.WebhookBaseUrl.TrimEnd('/') +
                    pathAndQuery;
            }

            return
                $"{Request.Scheme}://{Request.Host}{pathAndQuery}";
        }

        private string BuildWebhookUrl(
            string path)
        {
            if (!string.IsNullOrWhiteSpace(
                    _options.WebhookBaseUrl))
            {
                return
                    _options.WebhookBaseUrl.TrimEnd('/') +
                    path;
            }

            return
                $"{Request.Scheme}://{Request.Host}{path}";
        }

        private static ContentResult Twiml(
            string xml)
        {
            return new ContentResult
            {
                Content = xml.Trim(),
                ContentType = "text/xml; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static string Xml(
            string? value)
        {
            return SecurityElement.Escape(
                       value ?? string.Empty)
                   ?? string.Empty;
        }
    }
}