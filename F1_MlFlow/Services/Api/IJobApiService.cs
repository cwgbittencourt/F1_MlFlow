using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Jobs;

namespace F1_MlFlow.Services.Api;

public interface IJobApiService
{
    Task<ApiResult<IReadOnlyList<JobSummaryDto>>> GetRecentJobsAsync(CancellationToken cancellationToken = default);
    Task<ApiResult<JobDetailsDto>> GetJobDetailsAsync(string jobId, CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<JobLogLineDto>>> GetJobLogsAsync(string jobId, int lines = 200, CancellationToken cancellationToken = default);
}
