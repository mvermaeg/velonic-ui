namespace MyApp.Api.Data.Entities;
public sealed class ThumbtackCoverageImportBatch
{
    public long Id { get; set; }
    public string Fingerprint { get; set; } = "";
    public string SourceName { get; set; } = "";
    public DateTime ImportedOn { get; set; }
    public int SourceRowCount { get; set; }
    public int NormalizedZipCount { get; set; }
    public int PaddedRowCount { get; set; }
    public int InsertedPairCount { get; set; }
    public string ServicesJson { get; set; } = "";
    public string SourceRowsJson { get; set; } = "";
}
