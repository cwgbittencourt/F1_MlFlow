namespace F1_MlFlow.Models.Gold;

public sealed class DriverProfileDto
{
    public string? DriverName { get; set; }
    public int? DriverNumber { get; set; }
    public double? LapMean { get; set; }
    public double? LapStd { get; set; }
    public double? AnomalyRate { get; set; }
    public double? FinishRate { get; set; }
    public double? PointsTotal { get; set; }
    public int? MeetingsTotal { get; set; }
    public double? DegradationMean { get; set; }
    public double? DeltaPaceMean { get; set; }
    public double? RankPercentileMean { get; set; }
}
