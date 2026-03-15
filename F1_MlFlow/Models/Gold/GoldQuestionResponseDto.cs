namespace F1_MlFlow.Models.Gold;

public sealed class GoldQuestionResponseDto
{
    public string? Answer { get; set; }
    public string? RawResponse { get; set; }
    public Dictionary<string, string>? Context { get; set; }
    public DateTimeOffset? AnsweredAt { get; set; }
}
