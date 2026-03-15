using System.Text.Json.Serialization;

namespace F1_MlFlow.Models.Mlflow;

public sealed class MlflowSearchRunsRequest
{
    [JsonPropertyName("experiment_ids")]
    public List<string> ExperimentIds { get; set; } = new();

    [JsonPropertyName("max_results")]
    public int? MaxResults { get; set; }

    [JsonPropertyName("order_by")]
    public List<string>? OrderBy { get; set; }
}

public sealed class MlflowSearchRunsResponse
{
    [JsonPropertyName("runs")]
    public List<MlflowRunRecord>? Runs { get; set; }

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

public sealed class MlflowRunRecord
{
    [JsonPropertyName("info")]
    public MlflowRunInfo? Info { get; set; }

    [JsonPropertyName("data")]
    public MlflowRunData? Data { get; set; }
}

public sealed class MlflowRunInfo
{
    [JsonPropertyName("run_id")]
    public string? RunId { get; set; }

    [JsonPropertyName("run_uuid")]
    public string? RunUuid { get; set; }

    [JsonPropertyName("experiment_id")]
    public string? ExperimentId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("start_time")]
    public long? StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public long? EndTime { get; set; }

    [JsonPropertyName("run_name")]
    public string? RunName { get; set; }
}

public sealed class MlflowRunData
{
    [JsonPropertyName("metrics")]
    public List<MlflowKeyValueDouble>? Metrics { get; set; }

    [JsonPropertyName("params")]
    public List<MlflowKeyValueString>? Params { get; set; }

    [JsonPropertyName("tags")]
    public List<MlflowKeyValueString>? Tags { get; set; }
}

public sealed class MlflowKeyValueDouble
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("step")]
    public long? Step { get; set; }
}

public sealed class MlflowKeyValueString
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
