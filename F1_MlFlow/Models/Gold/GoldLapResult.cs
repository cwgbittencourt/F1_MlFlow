using F1_MlFlow.Models.Catalog;

namespace F1_MlFlow.Models.Gold;

public sealed class GoldLapResult
{
    public IReadOnlyList<GoldRowDto> Items { get; init; } = Array.Empty<GoldRowDto>();
    public int? MaxLapNumber { get; init; }
}
