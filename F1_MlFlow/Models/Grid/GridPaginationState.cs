namespace F1_MlFlow.Models.Grid;

public sealed class GridPaginationState
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public void EnsureBounds()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (TotalPages > 0 && PageNumber > TotalPages)
        {
            PageNumber = TotalPages;
        }
    }
}
