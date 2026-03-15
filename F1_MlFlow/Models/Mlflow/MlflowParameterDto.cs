namespace F1_MlFlow.Models.Mlflow;

public sealed class MlflowParameterDto
{
    public string Key { get; set; } = default!;
    public string? Value { get; set; }
}
