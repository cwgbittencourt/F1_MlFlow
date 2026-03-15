using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;

namespace F1_MlFlow.Services.Api;

public interface IDriverProfileApiService
{
    Task<ApiResult<IReadOnlyList<DriverProfileDto>>> GetProfilesAsync(int? season = null, CancellationToken cancellationToken = default);
}
