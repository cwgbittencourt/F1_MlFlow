namespace F1_MlFlow.Models.Grid;

public sealed class GridQueryState
{
    public string? GlobalFilter { get; set; }
    public Dictionary<string, string?> ColumnFilters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public GridSortState Sort { get; set; } = new();
    public GridPaginationState Pagination { get; set; } = new();

    public GridQueryState Clone()
    {
        return new GridQueryState
        {
            GlobalFilter = GlobalFilter,
            ColumnFilters = new Dictionary<string, string?>(ColumnFilters, StringComparer.OrdinalIgnoreCase),
            Sort = new GridSortState
            {
                ColumnKey = Sort.ColumnKey,
                Direction = Sort.Direction
            },
            Pagination = new GridPaginationState
            {
                PageNumber = Pagination.PageNumber,
                PageSize = Pagination.PageSize,
                TotalItems = Pagination.TotalItems
            }
        };
    }
}
