namespace F1_MlFlow.Models.Grid;

public sealed class GridDataPage<TItem>
{
    public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();
    public int TotalItems { get; set; }
}
