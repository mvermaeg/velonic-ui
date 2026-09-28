namespace MyApp.Api.Data.Entities;

public sealed class ThumbtackCoverageService
{
    public string ServiceCode { get; set; } = null!;
    public string CategoryPk { get; set; } = null!;
    public bool IsEnabled { get; set; }
}

public sealed class ThumbtackZipCoverage
{
    public string ServiceCode { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public long ImportBatchId { get; set; }
}
