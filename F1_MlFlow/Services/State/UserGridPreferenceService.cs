using System.Text.Json;
using F1_MlFlow.Models.Grid;
using Microsoft.JSInterop;

namespace F1_MlFlow.Services.State;

public sealed class UserGridPreferenceService(IJSRuntime jsRuntime) : IUserGridPreferenceService
{
    private const string StoragePrefix = "f1mlflow.grid.";
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<UserGridPreference?> GetPreferencesAsync(string gridKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gridKey))
        {
            return null;
        }

        var key = BuildKey(gridKey);

        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<UserGridPreference>(json, SerializerOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SavePreferencesAsync(UserGridPreference preference, CancellationToken cancellationToken = default)
    {
        if (preference is null || string.IsNullOrWhiteSpace(preference.GridKey))
        {
            return;
        }

        var key = BuildKey(preference.GridKey);
        var json = JsonSerializer.Serialize(preference, SerializerOptions);

        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, json);
        }
        catch
        {
            // Ignorar falhas de storage para não quebrar a UI.
        }
    }

    private static string BuildKey(string gridKey) => $"{StoragePrefix}{gridKey}";
}
