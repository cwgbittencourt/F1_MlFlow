namespace F1_MlFlow.Models.Grid;

public sealed class UserGridPreference
{
    public string GridKey { get; set; } = default!;
    public List<string> VisibleColumns { get; set; } = new();
    public GridSortState Sort { get; set; } = new();
    public int PageSize { get; set; } = 20;
    public string? LastTab { get; set; }
}
