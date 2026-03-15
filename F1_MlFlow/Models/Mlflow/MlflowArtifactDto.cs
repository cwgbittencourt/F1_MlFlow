namespace F1_MlFlow.Models.Mlflow;

public sealed class MlflowArtifactDto
{
    public string? Path { get; set; }
    public string? ArtifactUri { get; set; }
    public string? FileType { get; set; }
    public long? Size { get; set; }
}
