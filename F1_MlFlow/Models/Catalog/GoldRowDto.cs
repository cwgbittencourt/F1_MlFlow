namespace F1_MlFlow.Models.Catalog;

public sealed class GoldRowDto
{
    public int? Season { get; set; }
    public string? MeetingKey { get; set; }
    public string? MeetingName { get; set; }
    public DateTimeOffset? MeetingDateStart { get; set; }
    public string? SessionKey { get; set; }
    public string? SessionName { get; set; }
    public int? DriverNumber { get; set; }
    public string? DriverName { get; set; }
    public string? TeamName { get; set; }
    public int? LapNumber { get; set; }
    public double? LapDuration { get; set; }
    public double? DurationSector1 { get; set; }
    public double? DurationSector2 { get; set; }
    public double? DurationSector3 { get; set; }
    public double? AvgSpeed { get; set; }
    public double? MaxSpeed { get; set; }
    public double? MinSpeed { get; set; }
    public double? SpeedStd { get; set; }
    public double? AvgRpm { get; set; }
    public double? MaxRpm { get; set; }
    public double? MinRpm { get; set; }
    public double? RpmStd { get; set; }
    public double? AvgThrottle { get; set; }
    public double? MaxThrottle { get; set; }
    public double? MinThrottle { get; set; }
    public double? ThrottleStd { get; set; }
    public double? FullThrottlePct { get; set; }
    public double? BrakePct { get; set; }
    public int? BrakeEvents { get; set; }
    public int? HardBrakeEvents { get; set; }
    public double? DrsPct { get; set; }
    public int? GearChanges { get; set; }
    public double? DistanceTraveled { get; set; }
    public double? TrajectoryLength { get; set; }
    public double? TrajectoryVariation { get; set; }
    public int? TelemetryPoints { get; set; }
    public int? TrajectoryPoints { get; set; }
    public bool? HasTelemetry { get; set; }
    public bool? HasTrajectory { get; set; }
    public int? StintNumber { get; set; }
    public string? Compound { get; set; }
    public int? StintLapStart { get; set; }
    public int? StintLapEnd { get; set; }
    public int? TyreAgeAtStart { get; set; }
    public int? TyreAgeAtLap { get; set; }
    public double? TrackTemperature { get; set; }
    public double? AirTemperature { get; set; }
    public DateTimeOffset? WeatherDate { get; set; }
    public bool? IsPitOutLap { get; set; }
    public string? StoragePath { get; set; }
}
