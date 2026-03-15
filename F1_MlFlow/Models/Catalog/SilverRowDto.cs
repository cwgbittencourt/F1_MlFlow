namespace F1_MlFlow.Models.Catalog;

public sealed class SilverRowDto
{
    public int? Season { get; set; }
    public string? MeetingKey { get; set; }
    public string? SessionKey { get; set; }
    public int? DriverNumber { get; set; }
    public string? DatasetName { get; set; }
    public DateTimeOffset? NormalizedAt { get; set; }
    public string? SchemaVersion { get; set; }
    public int? RecordCount { get; set; }
    public double? NullPct { get; set; }
    public string? StoragePath { get; set; }
}
