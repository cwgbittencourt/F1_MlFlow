namespace F1_MlFlow.Models.Health;

public sealed class HealthDependencyDto
{
    public string Name { get; set; } = default!;
    public string Status { get; set; } = "unknown";
    public double? LatencyMs { get; set; }
    public string? Message { get; set; }
}
