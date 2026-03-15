using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Health;

namespace F1_MlFlow.Services.Api;

public interface IHealthApiService
{
    Task<ApiResult<HealthStatusDto>> GetHealthAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<HealthDependencyDto>>> GetDependenciesAsync(CancellationToken cancellationToken = default);
}
