namespace F1_MlFlow.Models.Mlflow;

public sealed class MlflowRunDto
{
    public string? ExperimentName { get; set; }
    public string RunId { get; set; } = default!;
    public string? RunName { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public double? Duration { get; set; }
    public List<MlflowMetricDto> Metrics { get; set; } = new();
    public List<MlflowParameterDto> Parameters { get; set; } = new();
    public List<MlflowArtifactDto> Artifacts { get; set; } = new();
}
