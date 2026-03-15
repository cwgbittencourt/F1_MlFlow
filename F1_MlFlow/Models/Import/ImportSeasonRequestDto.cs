using System.Text.Json.Serialization;

namespace F1_MlFlow.Models.Import;

public sealed class ImportSeasonRequestDto
{
    [JsonPropertyName("season")]
    public int Season { get; set; }

    [JsonPropertyName("session_name")]
    public string SessionName { get; set; } = "Race";

    [JsonPropertyName("include_llm")]
    public bool IncludeLlm { get; set; } = true;

    [JsonPropertyName("llm_endpoint")]
    public string? LlmEndpoint { get; set; }

    [JsonPropertyName("resume_job_id")]
    public string? ResumeJobId { get; set; }
}
