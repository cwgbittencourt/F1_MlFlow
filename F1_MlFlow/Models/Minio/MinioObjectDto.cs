namespace F1_MlFlow.Models.Minio;

public sealed class MinioObjectDto
{
    public string? Bucket { get; set; }
    public string? Prefix { get; set; }
    public string? ObjectName { get; set; }
    public string? ObjectType { get; set; }
    public long? Size { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? Layer { get; set; }
    public string? StorageUri { get; set; }
}
