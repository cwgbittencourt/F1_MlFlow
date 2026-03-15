namespace F1_MlFlow.Models.Gold;

public sealed class GoldQuestionRequestDto
{
    public string Question { get; set; } = default!;
    public int? Season { get; set; }
    public string? SessionName { get; set; }
    public string? MeetingKey { get; set; }
    public int? DriverNumber { get; set; }
}
