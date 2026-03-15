namespace F1_MlFlow.Services.State;

public interface IAdminSessionService
{
    Task<bool> IsAuthorizedAsync(CancellationToken cancellationToken = default);
    Task SetAuthorizedAsync(bool authorized, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
