namespace F1_MlFlow.Models.Jobs;

public sealed class JobSummaryDto
{
    public string JobId { get; set; } = default!;
    public string? JobType { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public double? Duration { get; set; }
    public string? LastMessage { get; set; }
}
