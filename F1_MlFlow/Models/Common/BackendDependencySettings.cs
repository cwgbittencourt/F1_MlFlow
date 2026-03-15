namespace F1_MlFlow.Models.Common;

public sealed class BackendDependencySettings
{
    public string? MlflowTrackingUri { get; set; }
    public string? MlflowS3EndpointUrl { get; set; }
    public string? MlflowExperimentId { get; set; }
    public string? DataLakeBucket { get; set; }
    public string? DataLakeS3EndpointUrl { get; set; }
    public string? DataLakePrefix { get; set; }
    public bool DataLakeCreateBucket { get; set; }
    public string? DataLakeSubdirs { get; set; }
    public string? DataLakeDownloadSubdirs { get; set; }
}
