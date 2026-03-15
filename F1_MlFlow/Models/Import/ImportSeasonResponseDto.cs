using System.Text.Json.Serialization;

namespace F1_MlFlow.Models.Import;

public sealed class ImportSeasonResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
