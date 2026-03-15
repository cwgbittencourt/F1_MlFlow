namespace F1_MlFlow.Models.Catalog;

public sealed class BronzeRowDto
{
    public int? Season { get; set; }
    public string? MeetingKey { get; set; }
    public string? SessionKey { get; set; }
    public int? DriverNumber { get; set; }
    public string? SourceEndpoint { get; set; }
    public string? FileName { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public int? RecordCount { get; set; }
    public string? StoragePath { get; set; }
    public string? SyncStatus { get; set; }
}
