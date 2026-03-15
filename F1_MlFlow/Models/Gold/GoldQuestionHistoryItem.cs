namespace F1_MlFlow.Models.Gold;

public sealed class GoldQuestionHistoryItem
{
    public string Question { get; set; } = default!;
    public GoldQuestionResponseDto? Response { get; set; }
    public GoldQuestionRequestDto? Context { get; set; }
    public DateTimeOffset AskedAt { get; set; }
}
