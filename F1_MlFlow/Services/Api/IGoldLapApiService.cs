using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;

namespace F1_MlFlow.Services.Api;

public interface IGoldLapApiService
{
    Task<ApiResult<GoldLapResult>> GetLapAsync(int season, int lapNumber, string? meetingKey, string? meetingName, CancellationToken cancellationToken = default);
}
