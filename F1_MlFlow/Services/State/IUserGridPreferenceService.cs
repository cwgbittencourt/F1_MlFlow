using F1_MlFlow.Models.Grid;

namespace F1_MlFlow.Services.State;

public interface IUserGridPreferenceService
{
    Task<UserGridPreference?> GetPreferencesAsync(string gridKey, CancellationToken cancellationToken = default);
    Task SavePreferencesAsync(UserGridPreference preference, CancellationToken cancellationToken = default);
}
