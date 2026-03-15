using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class DriverProfileApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IDriverProfileApiService
{
    public Task<ApiResult<IReadOnlyList<DriverProfileDto>>> GetProfilesAsync(int? season = null, CancellationToken cancellationToken = default)
    {
        // TODO: ajustar contrato conforme payload esperado pela API de perfis.
        if (season is null)
        {
            return PostAsync<object, IReadOnlyList<DriverProfileDto>>("/driver-profiles", new { }, cancellationToken);
        }

        return PostAsync<object, IReadOnlyList<DriverProfileDto>>("/driver-profiles/season", new { season }, cancellationToken);
    }
}
