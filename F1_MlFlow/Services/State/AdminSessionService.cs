using Microsoft.JSInterop;

namespace F1_MlFlow.Services.State;

public sealed class AdminSessionService(IJSRuntime jsRuntime) : IAdminSessionService
{
    private const string StorageKey = "f1mlflow.admin.auth";

    public async Task<bool> IsAuthorizedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, StorageKey);
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task SetAuthorizedAsync(bool authorized, CancellationToken cancellationToken = default)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, StorageKey, authorized ? "true" : "false");
        }
        catch
        {
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, StorageKey);
        }
        catch
        {
        }
    }
}
