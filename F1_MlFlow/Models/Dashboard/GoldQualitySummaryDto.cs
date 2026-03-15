namespace F1_MlFlow.Models.Dashboard;

public sealed class GoldQualitySummaryDto
{
    public int Rows { get; set; }
    public double? NullPct { get; set; }
    public int ValidLaps { get; set; }
    public int DiscardedLaps { get; set; }
    public List<GoldQualityGroupDto> Groups { get; set; } = new();
}

public sealed class GoldQualityGroupDto
{
    public int? Season { get; set; }
    public string? Meeting { get; set; }
    public string? Session { get; set; }
    public int Rows { get; set; }
    public double? NullPct { get; set; }
    public int ValidLaps { get; set; }
    public int DiscardedLaps { get; set; }
}
