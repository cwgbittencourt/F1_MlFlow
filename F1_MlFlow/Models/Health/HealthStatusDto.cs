namespace F1_MlFlow.Models.Health;

public sealed class HealthStatusDto
{
    public string Status { get; set; } = "unknown";
    public double? LatencyMs { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}
