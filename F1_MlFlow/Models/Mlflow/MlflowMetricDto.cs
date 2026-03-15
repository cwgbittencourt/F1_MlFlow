namespace F1_MlFlow.Models.Mlflow;

public sealed class MlflowMetricDto
{
    public string Key { get; set; } = default!;
    public double? Value { get; set; }
}
