namespace F1_MlFlow.Models.Dashboard;

public sealed class DashboardSummaryDto
{
    public int BronzeCount { get; set; }
    public int SilverCount { get; set; }
    public int GoldCount { get; set; }
    public int MeetingsCount { get; set; }
    public int SessionsCount { get; set; }
    public int DriversCount { get; set; }
    public int RunningJobsCount { get; set; }
    public int CompletedJobsCount { get; set; }
    public int FailedJobsCount { get; set; }
    public int MlflowRunsCount { get; set; }
    public int ArtifactCount { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}
