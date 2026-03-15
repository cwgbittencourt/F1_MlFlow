namespace F1_MlFlow.Models.Grid;

public sealed class GridSortState
{
    public string? ColumnKey { get; set; }
    public GridSortDirection Direction { get; set; } = GridSortDirection.None;

    public bool HasSort => !string.IsNullOrWhiteSpace(ColumnKey) && Direction != GridSortDirection.None;
}
