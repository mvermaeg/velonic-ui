using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using MyApp.Api.Services.Bidding;
using MyApp.Api.Services.Deliveries;
using MyApp.Api.Services.EmailValidation;
using MyApp.Api.Services.ExternalDeliveries;
using MyApp.Api.Services.Fraud;

namespace MyApp.Api.Services.MetaLeads
{
    public class MetaLeadService
    {
        private readonly HttpClient _httpClient;
        private readonly MyAppDbContext _db;
        private readonly MetaLeadOptions _options;

        private readonly BouncerEmailValidationService _emailValidation;
        private readonly ILeadFraudService _leadFraudService;
        private readonly LeadBiddingService _leadBiddingService;
        private readonly LeadDeliveryService _leadDeliveryService;
        private readonly ExternalLeadDistributionService
            _externalLeadDistributionService;

        private readonly ILogger<MetaLeadService> _logger;

        public MetaLeadService(
            HttpClient httpClient,
            MyAppDbContext db,
            IOptions<MetaLeadOptions> options,
            BouncerEmailValidationService emailValidation,
            ILeadFraudService leadFraudService,
            LeadBiddingService leadBiddingService,
            LeadDeliveryService leadDeliveryService,
            ExternalLeadDistributionService externalLeadDistributionService,
            ILogger<MetaLeadService> logger)
        {
            _httpClient = httpClient;
            _db = db;
            _options = options.Value;
            _emailValidation = emailValidation;
            _leadFraudService = leadFraudService;
            _leadBiddingService = leadBiddingService;
            _leadDeliveryService = leadDeliveryService;
            _externalLeadDistributionService =
                externalLeadDistributionService;
            _logger = logger;
        }

        public async Task ProcessLeadAsync(
            string pageId,
            string formId,
            string leadgenId,
            string webhookJson,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return;

            if (string.IsNullOrWhiteSpace(leadgenId))
                return;

            // Meta retries webhooks. Do not create duplicate leads.
            var alreadyProcessed =
                await _db.LeadRawPayloads.AnyAsync(
                    x =>
                        x.SourceName == "Meta" &&
                        x.ExternalLeadId == leadgenId,
                    cancellationToken);

            if (alreadyProcessed)
            {
                _logger.LogInformation(
                    "Meta lead {LeadgenId} already processed.",
                    leadgenId);

                return;
            }

            if (!_options.Pages.TryGetValue(
                    pageId,
                    out var page))
            {
                _logger.LogWarning(
                    "Meta PageId {PageId} is not configured.",
                    pageId);

                return;
            }

            if (string.IsNullOrWhiteSpace(page.AccessToken))
            {
                _logger.LogError(
                    "Meta access token missing for PageId {PageId}.",
                    pageId);

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    _options.GraphApiVersion))
            {
                throw new InvalidOperationException(
                    "MetaLeadAds:GraphApiVersion is missing.");
            }

            var graphUrl =
                $"https://graph.facebook.com/" +
                $"{_options.GraphApiVersion.Trim('/')}/" +
                $"{Uri.EscapeDataString(leadgenId)}" +
                "?fields=id,created_time,ad_id,ad_name," +
                "adset_id,adset_name,campaign_id,campaign_name," +
                "form_id,field_data" +
                $"&access_token={Uri.EscapeDataString(page.AccessToken)}";

            using var response =
                await _httpClient.GetAsync(
                    graphUrl,
                    cancellationToken);

            var leadJson =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Meta lead retrieval failed. LeadgenId={LeadgenId}, HTTP={HttpStatus}, Response={Response}",
                    leadgenId,
                    (int)response.StatusCode,
                    leadJson);

                return;
            }

            using var document =
                JsonDocument.Parse(leadJson);

            var root = document.RootElement;

            var fields = ReadFieldData(root);

            var fullName =
                GetFirst(
                    fields,
                    "full_name",
                    "fullname",
                    "name");

            var firstName =
                GetFirst(fields, "first_name", "firstname");

            var lastName =
                GetFirst(fields, "last_name", "lastname");

            if (string.IsNullOrWhiteSpace(fullName))
            {
                fullName =
                    string.Join(
                        " ",
                        new[] { firstName, lastName }
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x)))
                    .Trim();
            }

            var email =
                GetFirst(fields, "email");

            var phone =
                GetFirst(
                    fields,
                    "phone_number",
                    "phone",
                    "mobile_number",
                    "mobile");

            var address =
                GetFirst(
                    fields,
                    "street_address",
                    "address",
                    "full_address");

            var postcode =
                GetFirst(
                    fields,
                    "zip_code",
                    "zipcode",
                    "postal_code",
                    "postcode");

            var city =
                GetFirst(fields, "city");

            var state =
                GetFirst(
                    fields,
                    "state",
                    "state_province");

            var country =
                GetFirst(fields, "country");

            if (string.IsNullOrWhiteSpace(country))
                country = "United States";

            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = new string(
                    phone.Where(char.IsDigit).ToArray());

                if (phone.Length == 11 &&
                    phone.StartsWith("1"))
                {
                    phone = phone[1..];
                }
            }

            var campaignName =
                GetJsonString(root, "campaign_name")
                ?? "Meta Lead Ads";

            var actualFormId =
                GetJsonString(root, "form_id")
                ?? formId;

            var leadTypeId =
                ResolveLeadTypeId(
                    page,
                    actualFormId);

            if (leadTypeId <= 0)
            {
                _logger.LogError(
                    "No LeadTypeId mapping for Meta PageId {PageId}, FormId {FormId}.",
                    pageId,
                    actualFormId);

                return;
            }

            // Verify email if Meta supplied one.
            string emailStatus = "NotProvided";
            string emailReason = "Meta lead did not include email.";
            bool isEmailDeliverable = false;

            //if (!string.IsNullOrWhiteSpace(email))
            //{
            //    try
            //    {
            //        var emailCheck =
            //            await _emailValidation.VerifyAsync(email);

            //        emailStatus =
            //            emailCheck?.Status ?? "NotVerified";

            //        emailReason =
            //            emailCheck?.Reason ??
            //            "Email verification unavailable.";

            //        isEmailDeliverable =
            //            emailCheck?.Deliverable ?? false;
            //    }
            //    catch (Exception ex)
            //    {
            //        emailStatus = "NotVerified";
            //        emailReason = ex.Message;
            //    }
            //}

            if (!string.IsNullOrWhiteSpace(email))
            {
                emailStatus = "NotVerified";
                emailReason = "Email verification skipped for Meta lead test.";
                isEmailDeliverable = false;
            }

            var additionalData =
                fields
                    .Where(x =>
                        !IsStandardField(x.Key))
                    .ToDictionary(
                        x => x.Key,
                        x => (object?)x.Value);

            additionalData["metaLeadgenId"] =
                leadgenId;

            additionalData["metaPageId"] =
                pageId;

            additionalData["metaFormId"] =
                actualFormId;

            additionalData["metaCampaignId"] =
                GetJsonString(root, "campaign_id");

            additionalData["metaAdId"] =
                GetJsonString(root, "ad_id");

            additionalData["metaAdName"] =
                GetJsonString(root, "ad_name");

            additionalData["metaAdsetId"] =
                GetJsonString(root, "adset_id");

            additionalData["metaAdsetName"] =
                GetJsonString(root, "adset_name");

            var ownsPropertyText =
                GetFirst(
                    fields,
                    "own_property",
                    "owns_property",
                    "do_you_own_your_home",
                    "are_you_the_homeowner");

            bool? ownsProperty =
                ParseBooleanAnswer(
                    ownsPropertyText);

            var lead = new Lead
            {
                LeadUuid = Guid.NewGuid(),

                CreatedAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow,

                FingerprintHash =
                    $"META-{leadgenId}",

                FullName =
                    string.IsNullOrWhiteSpace(fullName)
                        ? "Meta Lead"
                        : fullName,

                Email = email,
                Phone = phone,

                // Do NOT use webhook server IP as consumer IP.
                IpAddress = null,
                UserAgent = null,

                VisitorCountry = country,

                CampaignName = campaignName,
                AffiliateName = "Meta",
                PageName = page.PageName,

                Address = address,
                Postcode = postcode,
                State = state,
                City = city,
                Country = country,

                Step = 1,
                IsTest = false,
                IsCompleted = true,

                LeadTypeId = leadTypeId,

                LeadStatus = "New",
                IsDeleted = false,

                EmailVerificationStatus =
                    emailStatus,

                EmailVerificationReason =
                    emailReason,

                IsEmailDeliverable =
                    isEmailDeliverable,

                OwnsProperty =
                    ownsProperty,

                LandingPageUrl =
                    $"Meta Lead Form {actualFormId}",

                AdditionalDataJson =
                    JsonSerializer.Serialize(
                        additionalData),

                // Do NOT claim TCPA compliance automatically.
                IsTcpaCompliant = false,

                ConsentCapturedOn =
                    TryGetCreatedTime(root)
            };

            _db.Leads.Add(lead);

            await _db.SaveChangesAsync(
                cancellationToken);

            var rawPayload =
                new LeadRawPayload
                {
                    LeadId = lead.Id,

                    SourceName = "Meta",

                    ExternalLeadId =
                        leadgenId,

                    RawJson =
                        JsonSerializer.Serialize(
                            new
                            {
                                webhook = webhookJson,
                                metaLead = leadJson
                            }),

                    ReceivedOn =
                        DateTime.UtcNow
                };

            _db.LeadRawPayloads.Add(
                rawPayload);

            await _db.SaveChangesAsync(
                cancellationToken);

            // Same downstream process already used by Website leads.
            await _leadFraudService
                .CheckAndApplyAsync(lead);

            await _db.SaveChangesAsync(
                cancellationToken);

            await _leadBiddingService
                .RunForLeadAsync(lead.Id);

            await _externalLeadDistributionService
                .QueueForLeadAsync(
                    lead.Id,
                    cancellationToken);

            var winningBid =
                await _db.LeadBiddingResults
                    .Where(x =>
                        x.LeadId == lead.Id &&
                        x.IsWon &&
                        !x.IsSold)
                    .OrderByDescending(
                        x => x.BidAmount)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (winningBid != null)
            {
                await _leadDeliveryService
                    .CreateDeliveryFromBiddingResultAsync(
                        winningBid.Id);
            }

            _logger.LogInformation(
                "Meta Lead received. MetaLeadgenId={LeadgenId}, HomeyyLeadId={LeadId}, Page={PageName}",
                leadgenId,
                lead.Id,
                page.PageName);
        }

        //private static int ResolveLeadTypeId(
        //    MetaPageOptions page,
        //    string? formId)
        //{
        //    if (!string.IsNullOrWhiteSpace(formId) &&
        //        page.FormLeadTypeIds.TryGetValue(
        //            formId,
        //            out var mappedLeadTypeId))
        //    {
        //        return mappedLeadTypeId;
        //    }

        //    return page.LeadTypeId;
        //}


        private static int ResolveLeadTypeId(
    MetaPageOptions page,
    string? formId)
        {
            if (!string.IsNullOrWhiteSpace(formId) &&
                page.FormLeadTypeIds.TryGetValue(
                    formId,
                    out var mappedLeadTypeId))
            {
                return mappedLeadTypeId;
            }

            return page.LeadTypeId;
        }

        private static Dictionary<string, string?>
            ReadFieldData(
                JsonElement root)
        {
            var result =
                new Dictionary<string, string?>(
                    StringComparer.OrdinalIgnoreCase);

            if (!root.TryGetProperty(
                    "field_data",
                    out var fieldData) ||
                fieldData.ValueKind !=
                    JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in
                     fieldData.EnumerateArray())
            {
                var name =
                    GetJsonString(
                        item,
                        "name");

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string? value = null;

                if (item.TryGetProperty(
                        "values",
                        out var values) &&
                    values.ValueKind ==
                        JsonValueKind.Array)
                {
                    value =
                        string.Join(
                            ", ",
                            values
                                .EnumerateArray()
                                .Select(x =>
                                    x.ValueKind ==
                                    JsonValueKind.String
                                        ? x.GetString()
                                        : x.ToString())
                                .Where(x =>
                                    !string.IsNullOrWhiteSpace(x)));
                }

                result[name] = value;
            }

            return result;
        }

        private static string? GetFirst(
            Dictionary<string, string?> fields,
            params string[] names)
        {
            foreach (var name in names)
            {
                if (fields.TryGetValue(
                        name,
                        out var value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static string? GetJsonString(
            JsonElement element,
            string name)
        {
            if (!element.TryGetProperty(
                    name,
                    out var property))
            {
                return null;
            }

            return property.ValueKind ==
                JsonValueKind.String
                    ? property.GetString()
                    : property.ToString();
        }

        private static DateTime? TryGetCreatedTime(
            JsonElement root)
        {
            var value =
                GetJsonString(
                    root,
                    "created_time");

            return DateTime.TryParse(
                value,
                out var date)
                    ? date.ToUniversalTime()
                    : DateTime.UtcNow;
        }

        private static bool? ParseBooleanAnswer(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized =
                value.Trim().ToLowerInvariant();

            if (normalized is
                "yes" or
                "true" or
                "1" or
                "owner" or
                "homeowner")
            {
                return true;
            }

            if (normalized is
                "no" or
                "false" or
                "0" or
                "renter")
            {
                return false;
            }

            return null;
        }

        private static bool IsStandardField(
            string name)
        {
            var normalized =
                name.Trim().ToLowerInvariant();

            return normalized is
                "full_name" or
                "fullname" or
                "name" or
                "first_name" or
                "firstname" or
                "last_name" or
                "lastname" or
                "email" or
                "phone_number" or
                "phone" or
                "mobile" or
                "mobile_number" or
                "street_address" or
                "address" or
                "full_address" or
                "zip_code" or
                "zipcode" or
                "postal_code" or
                "postcode" or
                "city" or
                "state" or
                "state_province" or
                "country";
        }
    }
}