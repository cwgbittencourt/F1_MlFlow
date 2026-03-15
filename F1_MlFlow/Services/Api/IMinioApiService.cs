using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Minio;

namespace F1_MlFlow.Services.Api;

public interface IMinioApiService
{
    Task<ApiResult<IReadOnlyList<MinioObjectDto>>> GetObjectsAsync(int? limit = null, CancellationToken cancellationToken = default);
}
