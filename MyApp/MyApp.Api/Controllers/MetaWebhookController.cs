using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyApp.Api.Services.MetaLeads;

namespace MyApp.Api.Controllers
{
    [ApiController]
    [Route("api/meta/webhook")]
    [AllowAnonymous]
    public class MetaWebhookController : ControllerBase
    {
        private readonly MetaLeadService _metaLeadService;
        private readonly MetaLeadOptions _options;
        private readonly ILogger<MetaWebhookController> _logger;

        public MetaWebhookController(
            MetaLeadService metaLeadService,
            IOptions<MetaLeadOptions> options,
            ILogger<MetaWebhookController> logger)
        {
            _metaLeadService = metaLeadService;
            _options = options.Value;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Verify()
        {
            var mode = Request.Query["hub.mode"].ToString();
            var token = Request.Query["hub.verify_token"].ToString();
            var challenge = Request.Query["hub.challenge"].ToString();

            var verifyToken = _options.VerifyToken;

            if (mode == "subscribe" &&
                token == verifyToken)
            {
                return Content(
                    challenge,
                    "text/plain");
            }

            return Unauthorized(new
            {
                message = "Meta webhook verification failed.",
                modeReceived = mode,
                tokenMatched = token == verifyToken,
                verifyTokenConfigured =
                    !string.IsNullOrWhiteSpace(verifyToken)
            });
        }




        [HttpPost]
        public async Task<IActionResult> Receive(
     CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(
                Request.Body,
                Encoding.UTF8);

            var rawJson = await reader.ReadToEndAsync();

            _logger.LogInformation(
                "META WEBHOOK RECEIVED | Length={Length} | SignaturePresent={SignaturePresent} | AppSecretConfigured={AppSecretConfigured}",
                rawJson?.Length ?? 0,
                Request.Headers.ContainsKey("X-Hub-Signature-256"),
                !string.IsNullOrWhiteSpace(_options.AppSecret));

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                _logger.LogWarning(
                    "META WEBHOOK: Empty request body.");

                return Ok();
            }

            if (!ValidateMetaSignature(rawJson))
            {
                _logger.LogWarning(
                    "META WEBHOOK: Signature validation FAILED. AppSecretConfigured={AppSecretConfigured}",
                    !string.IsNullOrWhiteSpace(_options.AppSecret));

                return Unauthorized();
            }

            _logger.LogInformation(
                "META WEBHOOK: Signature VALID.");

            try
            {
                using var document =
                    JsonDocument.Parse(rawJson);

                var root =
                    document.RootElement;

                if (!root.TryGetProperty(
                        "entry",
                        out var entries) ||
                    entries.ValueKind !=
                        JsonValueKind.Array)
                {
                    _logger.LogWarning(
                        "META WEBHOOK: No entry array found.");

                    return Ok();
                }

                foreach (var entry in
                         entries.EnumerateArray())
                {
                    var entryPageId =
                        GetString(
                            entry,
                            "id");

                    if (!entry.TryGetProperty(
                            "changes",
                            out var changes) ||
                        changes.ValueKind !=
                            JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var change in
                             changes.EnumerateArray())
                    {
                        var field =
                            GetString(
                                change,
                                "field");

                        _logger.LogInformation(
                            "META WEBHOOK CHANGE | Field={Field}",
                            field);

                        if (!string.Equals(
                                field,
                                "leadgen",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!change.TryGetProperty(
                                "value",
                                out var value))
                        {
                            continue;
                        }

                        var leadgenId =
                            GetString(
                                value,
                                "leadgen_id");

                        var formId =
                            GetString(
                                value,
                                "form_id");

                        var pageId =
                            GetString(
                                value,
                                "page_id")
                            ?? entryPageId;

                        _logger.LogInformation(
                            "META LEAD EVENT | PageId={PageId} | FormId={FormId} | LeadgenId={LeadgenId}",
                            pageId,
                            formId,
                            leadgenId);

                        if (string.IsNullOrWhiteSpace(
                                leadgenId) ||
                            string.IsNullOrWhiteSpace(
                                pageId))
                        {
                            _logger.LogWarning(
                                "META LEAD EVENT ignored because PageId or LeadgenId was missing.");

                            continue;
                        }

                        await _metaLeadService
                            .ProcessLeadAsync(
                                pageId,
                                formId ?? string.Empty,
                                leadgenId,
                                rawJson,
                                cancellationToken);

                        _logger.LogInformation(
                            "META LEAD PROCESSED | PageId={PageId} | LeadgenId={LeadgenId}",
                            pageId,
                            leadgenId);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "META WEBHOOK processing failed.");

                return Ok();
            }
        }
        private bool ValidateMetaSignature(
            string rawJson)
        {
            if (string.IsNullOrWhiteSpace(
                    _options.AppSecret))
            {
                return false;
            }

            var signatureHeader =
                Request.Headers[
                    "X-Hub-Signature-256"]
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(
                    signatureHeader) ||
                !signatureHeader.StartsWith(
                    "sha256=",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var suppliedSignature =
                signatureHeader[
                    "sha256=".Length..];

            using var hmac =
                new HMACSHA256(
                    Encoding.UTF8.GetBytes(
                        _options.AppSecret));

            var calculatedBytes =
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(
                        rawJson));

            var calculatedSignature =
                Convert.ToHexString(
                    calculatedBytes)
                .ToLowerInvariant();

            var suppliedBytes =
                Encoding.UTF8.GetBytes(
                    suppliedSignature.ToLowerInvariant());

            var calculatedTextBytes =
                Encoding.UTF8.GetBytes(
                    calculatedSignature);

            return suppliedBytes.Length ==
                       calculatedTextBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(
                       suppliedBytes,
                       calculatedTextBytes);
        }

        private static string? GetString(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                    propertyName,
                    out var property))
            {
                return null;
            }

            return property.ValueKind ==
                JsonValueKind.String
                    ? property.GetString()
                    : property.ToString();
        }
    }
}