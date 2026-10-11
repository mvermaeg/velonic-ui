namespace MyApp.Api.Services.Routing;

// Persist only fixed classifications. Provider free text can echo contact data or credentials.
public static class ProviderFailureReason
{
    public static string Classify(string? text)
    {
        var value = text ?? "";
        bool Has(string term) => value.Contains(term, StringComparison.OrdinalIgnoreCase);
        if (Has("duplicate") || Has("already submitted")) return "Duplicate submission";
        if (Has("expired")) return "Expired request token";
        if (Has("invalid campaign") || Has("authentication") || Has("access key") || Has("unauthorized")) return "Authentication or campaign configuration";
        if (Has("no match") || Has("no buyer") || Has("no available buyer")) return "No buyer match";
        if (Has("filter")) return "Provider eligibility filter";
        if (Has("cap reached") || Has("rate limit")) return "Provider quota";
        if (Has("required") || Has("invalid") || Has("validation")) {
            foreach (var field in new[] { "email", "phone", "zip", "Project", "state", "city", "consent", "token" })
                if (System.Text.RegularExpressions.Regex.IsMatch(value,
                    @"\b(?:invalid|missing|required)\s+(?:field\s+)?" + field + @"\b|\b" + field + @"\s+(?:is\s+)?(?:invalid|missing|required)\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)) return "Provider validation: " + field;
            return "Provider request validation";
        }
        return "Unspecified provider rejection; provider logs required";
    }
}
