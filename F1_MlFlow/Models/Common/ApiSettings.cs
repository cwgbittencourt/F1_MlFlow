namespace F1_MlFlow.Models.Common;

public sealed class ApiSettings
{
    public string BaseUrl { get; set; } = "http://localhost:7077";
    public bool UseMockData { get; set; }
}
