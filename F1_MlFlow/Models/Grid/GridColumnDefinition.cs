namespace F1_MlFlow.Models.Grid;

public sealed class GridColumnDefinition
{
    public string Key { get; set; } = default!;
    public string Title { get; set; } = default!;
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public string DataType { get; set; } = "string";
    public string? Format { get; set; }
    public string? Width { get; set; }
    public string? CssClass { get; set; }
    public string? Alignment { get; set; }
}
