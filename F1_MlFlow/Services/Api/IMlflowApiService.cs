using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Mlflow;

namespace F1_MlFlow.Services.Api;

public interface IMlflowApiService
{
    Task<ApiResult<IReadOnlyList<MlflowRunDto>>> GetRunsAsync(int? limit = null, CancellationToken cancellationToken = default);
}
