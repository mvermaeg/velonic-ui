using Microsoft.EntityFrameworkCore;
using MyApp.Api.Data;
using MyApp.Api.Data.Entities;
using System.Text.RegularExpressions;

namespace MyApp.Api.Services.Fraud
{
    public class LeadFraudService : ILeadFraudService
    {
        private readonly MyAppDbContext _db;

        private static readonly HashSet<string> DisposableDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "mailinator.com",
            "tempmail.com",
            "10minutemail.com",
            "guerrillamail.com",
            "yopmail.com",
            "trashmail.com"
        };

        public LeadFraudService(MyAppDbContext db)
        {
            _db = db;
        }

        public async Task CheckAndApplyAsync(Lead lead)
        {
            var result = new FraudCheckResult();

            CheckEmail(lead, result);
            CheckBouncerEmailVerification(lead, result);
            CheckPostcodeCountryMismatch(lead, result);
            await CheckPostalAddressMatchAsync(lead, result);
            await CheckDuplicateAsync(lead, result);
            await CheckDuplicateDeviceAsync(lead, result);
            await CheckIpFrequencyAsync(lead, result);
            CheckLeadCompleteness(lead, result);
            CheckAffiliateRisk(lead, result);
            CheckVisitorCountryMismatch(lead, result);


            lead.FraudScore = result.Score;
            lead.LeadQualityScore = Math.Max(0, 100 - result.Score);
            lead.FraudReasons = string.Join(" | ", result.Reasons);
            lead.RiskCheckedOn = DateTime.UtcNow;

            lead.FraudLevel = result.Score switch
            {
                >= 70 => "High",
                >= 40 => "Medium",
                >= 20 => "Low",
                _ => "Clean"
            };

            lead.IsSuspicious = result.Score >= 40;
        }

        private static void CheckBouncerEmailVerification(Lead lead, FraudCheckResult result)
        {
            if (lead.IsEmailDeliverable == false)
            {
                result.Add(35, "Bouncer: email is not deliverable");
            }

            if (!string.IsNullOrWhiteSpace(lead.EmailVerificationStatus) &&
                lead.EmailVerificationStatus.Equals("risky", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(20, "Bouncer: email marked risky");
            }

            if (!string.IsNullOrWhiteSpace(lead.EmailVerificationStatus) &&
                lead.EmailVerificationStatus.Equals("undeliverable", StringComparison.OrdinalIgnoreCase))
            {
                lead.IsInvalidEmail = true;
                result.Add(35, "Bouncer: email undeliverable");
            }
        }

        private static void CheckVisitorCountryMismatch(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.Country) ||
                string.IsNullOrWhiteSpace(lead.VisitorCountry))
                return;

            var addressCountry = lead.Country.Trim().ToLower();
            var visitorCountry = lead.VisitorCountry.Trim().ToLower();

            if (!addressCountry.Contains(visitorCountry) &&
                !visitorCountry.Contains(addressCountry))
            {
                result.Add(20, $"Visitor country mismatch. Address country: {lead.Country}, IP country: {lead.VisitorCountry}");
            }
        }
        private async Task CheckDuplicateDeviceAsync(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.FingerprintHash))
                return;

            var since = DateTime.UtcNow.AddDays(-30);

            var duplicateDeviceCount = await _db.Leads.CountAsync(x =>
                x.Id != lead.Id &&
                !x.IsDeleted &&
                x.ReceivedAt >= since &&
                x.FingerprintHash == lead.FingerprintHash);

            if (duplicateDeviceCount >= 3)
            {
                result.Add(35, $"Multiple leads from same device/browser in last 30 days. Count: {duplicateDeviceCount}");
            }
        }
        private static void CheckEmail(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.Email))
            {
                result.Add(10, "Email missing");
                return;
            }

            var email = lead.Email.Trim();

            var valid = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            if (!valid)
            {
                lead.IsInvalidEmail = true;
                result.Add(30, "Invalid email format");
                return;
            }

            var domain = email.Split('@').LastOrDefault();

            lead.EmailDomain = domain;

            if (!string.IsNullOrWhiteSpace(domain) && DisposableDomains.Contains(domain))
            {
                lead.IsDisposableEmail = true;
                result.Add(25, "Disposable email domain");
            }
        }

        private static void CheckPostcodeCountryMismatch(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.Postcode))
                return;

            var postcode = lead.Postcode.Trim().ToUpperInvariant();

            var isUkPostcode = Regex.IsMatch(
                postcode,
                @"^([A-Z]{1,2}\d[A-Z\d]?\s*\d[A-Z]{2}|GIR\s*0AA)$",
                RegexOptions.IgnoreCase
            );

            if (!isUkPostcode)
            {
                lead.IsPostcodeCountryMismatch = true;
                result.Add(25, "Postcode does not match expected UK postcode format");
            }

            if (!string.IsNullOrWhiteSpace(lead.Country) &&
                !lead.Country.Equals("United Kingdom", StringComparison.OrdinalIgnoreCase) &&
                !lead.Country.Equals("UK", StringComparison.OrdinalIgnoreCase) &&
                isUkPostcode)
            {
                lead.IsPostcodeCountryMismatch = true;
                result.Add(20, "UK postcode submitted with non-UK country");
            }
        }

        private async Task CheckDuplicateAsync(Lead lead, FraudCheckResult result)
        {
            var since = DateTime.UtcNow.AddDays(-30);

            var duplicates = await _db.Leads
                .Where(x =>
                    x.Id != lead.Id &&
                    !x.IsDeleted &&
                    x.ReceivedAt >= since &&
                    (
                        (!string.IsNullOrWhiteSpace(lead.Email) && x.Email == lead.Email) ||
                        (!string.IsNullOrWhiteSpace(lead.Phone) && x.Phone == lead.Phone)
                    ))
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Email,
                    x.Phone
                })
                .ToListAsync();

            if (!duplicates.Any())
                return;

            lead.IsDuplicateLead = true;
            lead.DuplicateLeadCount = duplicates.Count;
            lead.LastDuplicateLeadId = duplicates.First().Id;

            result.Add(30, $"Duplicate lead found in last 30 days. Count: {duplicates.Count}");

            var emailDuplicate = !string.IsNullOrWhiteSpace(lead.Email) &&
                                 duplicates.Any(x => x.Email == lead.Email);

            var phoneDuplicate = !string.IsNullOrWhiteSpace(lead.Phone) &&
                                 duplicates.Any(x => x.Phone == lead.Phone);

            if (emailDuplicate && phoneDuplicate)
            {
                result.Add(20, "Both email and phone already exist");
            }
            else if (emailDuplicate)
            {
                result.Add(10, "Email already exists");
            }
            else if (phoneDuplicate)
            {
                result.Add(10, "Phone already exists");
            }
        }

        private async Task CheckPostalAddressMatchAsync(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.Postcode))
                return;

            var postcode = lead.Postcode.Trim().ToUpper();
            var city = lead.City?.Trim().ToLower();
            var state = lead.State?.Trim().ToLower();
            var country = lead.Country?.Trim().ToLower();

            var postalRecord = await _db.PostalCodeLocations
                .Where(x =>
                    x.IsActive &&
                    x.PostalCode.ToUpper() == postcode)
                .FirstOrDefaultAsync();

            if (postalRecord == null)
            {
                lead.IsPostalAddressMismatch = true;
                lead.PostalAddressCheckReason = "Postal code not found in location database.";
                result.Add(25, "Postal code not found in location database");
                return;
            }

            var cityMismatch =
                !string.IsNullOrWhiteSpace(city) &&
                !string.IsNullOrWhiteSpace(postalRecord.City) &&
                postalRecord.City.ToLower() != city;

            var stateMismatch =
                !string.IsNullOrWhiteSpace(state) &&
                (
                    (!string.IsNullOrWhiteSpace(postalRecord.StateCode) &&
                     postalRecord.StateCode.ToLower() != state) &&
                    (!string.IsNullOrWhiteSpace(postalRecord.StateName) &&
                     postalRecord.StateName.ToLower() != state)
                );

            if (cityMismatch || stateMismatch)
            {
                lead.IsPostalAddressMismatch = true;
                lead.PostalAddressCheckReason =
                    $"Submitted address does not match postal code. Expected: {postalRecord.City}, {postalRecord.StateCode}.";

                result.Add(30, lead.PostalAddressCheckReason);
            }
        }

        private async Task CheckIpFrequencyAsync(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.IpAddress))
                return;

            var since = DateTime.UtcNow.AddHours(-1);

            var count = await _db.Leads.CountAsync(x =>
                !x.IsDeleted &&
                x.IpAddress == lead.IpAddress &&
                x.ReceivedAt >= since);

            if (count >= 5)
            {
                result.Add(25, "High lead volume from same IP");
            }
        }

        private static void CheckLeadCompleteness(Lead lead, FraudCheckResult result)
        {
            if (string.IsNullOrWhiteSpace(lead.FullName))
                result.Add(10, "Full name missing");

            if (string.IsNullOrWhiteSpace(lead.Phone))
                result.Add(10, "Phone missing");

            if (!lead.IsCompleted)
                result.Add(10, "Lead journey not completed");

            if (lead.IsTest)
                result.Add(20, "Test lead");
        }

        private static void CheckAffiliateRisk(Lead lead, FraudCheckResult result)
        {
            if (lead.AffiliateId == null && !string.IsNullOrWhiteSpace(lead.AffiliateName))
                result.Add(10, "Affiliate name exists but AffiliateId missing");

            if (lead.AffiliateClickId == null &&
                lead.AffiliateClickUuid == null &&
                !string.IsNullOrWhiteSpace(lead.AffiliateSubId))
            {
                result.Add(10, "AffiliateSubId exists without click tracking");
            }
        }

        private class FraudCheckResult
        {
            public int Score { get; private set; }
            public List<string> Reasons { get; } = new();

            public void Add(int score, string reason)
            {
                Score += score;
                Reasons.Add(reason);
            }
        }
    }
}