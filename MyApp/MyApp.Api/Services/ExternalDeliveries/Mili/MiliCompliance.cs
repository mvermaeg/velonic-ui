using MyApp.Api.Data.Entities;
using System.Text.RegularExpressions;

namespace MyApp.Api.Services.ExternalDeliveries.Mili;

// Explicit documented ping fields only. Never pass the full contact-bearing lead to a ping.
public sealed record MiliPingCompliance(string? JornayaLeadId, string? CertificateUrl,
    bool? Consent, string? ConsentText, string? UserAgent)
{
    public static MiliPingCompliance From(Lead lead) => new(lead.JornayaLeadId,
        lead.TrustedFormCertificateUrl, lead.IsTcpaCompliant, lead.TcpaComplianceText, lead.UserAgent);
    public void Apply(Lead lead)
    {
        lead.JornayaLeadId = JornayaLeadId; lead.TrustedFormCertificateUrl = CertificateUrl;
        lead.IsTcpaCompliant = Consent; lead.TcpaComplianceText = ConsentText; lead.UserAgent = UserAgent;
    }
}

public sealed partial class MiliLeadProvider
{
    private static string? CertificateId(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme == "https" && uri.Host.Equals("cert.trustedform.com", StringComparison.OrdinalIgnoreCase) &&
        string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment) &&
        Regex.IsMatch(uri.AbsolutePath, "^/[a-fA-F0-9]{40}/?$") ? uri.AbsolutePath.Trim('/') : null;

    private static string? StateCode(string? value)
    {
        const string names = "Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|District of Columbia|Florida|Georgia|Hawaii|Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|New Hampshire|New Jersey|New Mexico|New York|North Carolina|North Dakota|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode Island|South Carolina|South Dakota|Tennessee|Texas|Utah|Vermont|Virginia|Washington|West Virginia|Wisconsin|Wyoming";
        var codes = "AL AK AZ AR CA CO CT DE DC FL GA HI ID IL IN IA KS KY LA ME MD MA MI MN MS MO MT NE NV NH NJ NM NY NC ND OH OK OR PA RI SC SD TN TX UT VT VA WA WV WI WY".Split(' ');
        var text = value?.Trim();
        if (codes.Contains(text?.ToUpperInvariant())) return text!.ToUpperInvariant();
        var index = Array.FindIndex(names.Split('|'), n => string.Equals(n, text, StringComparison.OrdinalIgnoreCase));
        return index < 0 ? null : codes[index];
    }

    private string? ValidatePingCompliance(Lead lead)
    {
        if (StateCode(lead.State) == null) return "Mili requires a valid two-letter US state.";
        if (string.IsNullOrWhiteSpace(lead.City)) return "Mili requires city.";
        if (!lead.OwnsProperty.HasValue) return "Mili requires home ownership answer.";
        if (!System.Net.IPAddress.TryParse(lead.IpAddress, out _)) return "Mili requires valid IP address.";
        if (!Guid.TryParse(lead.JornayaLeadId, out _)) return "Mili requires a captured valid Jornaya lead ID.";
        if (CertificateId(lead.TrustedFormCertificateUrl) == null) return "Mili requires a captured valid TrustedForm certificate.";
        if (lead.IsTcpaCompliant != true || string.IsNullOrWhiteSpace(lead.TcpaComplianceText)) return "Mili requires affirmative captured TCPA consent and language.";
        if (string.IsNullOrWhiteSpace(lead.UserAgent)) return "Mili requires captured user agent.";
        if (!Uri.TryCreate(lead.LandingPageUrl, UriKind.Absolute, out var page) || page.Scheme is not ("https" or "http")) return "Mili requires valid landing page.";
        if (string.IsNullOrWhiteSpace(_options.PubId) || string.IsNullOrWhiteSpace(_options.InternalSourceId)) return "Mili requires publisher and internal source configuration.";
        if (lead.Postcode?.Length != 5 || !lead.Postcode.All(char.IsAsciiDigit)) return "Mili requires five-digit ZIP.";
        return null;
    }
}
