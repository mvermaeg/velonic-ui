namespace MyApp.Api.Services.Routing;

public sealed class ExclusiveRoutingOptions
{
    public const string SectionName = "LeadRouting";
    public bool Enabled { get; set; } = false;
    public int PollSeconds { get; set; } = 15;
    public int MaxAttemptsPerDestination { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
}
