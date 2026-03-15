using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Health;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class HealthApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IHealthApiService
{
    public Task<ApiResult<HealthStatusDto>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<HealthStatusDto>("/health", cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<HealthDependencyDto>>> GetDependenciesAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<IReadOnlyList<HealthDependencyDto>>("/health/dependencies", cancellationToken);
    }
}
