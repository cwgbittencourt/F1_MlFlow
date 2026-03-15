using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Import;

namespace F1_MlFlow.Services.Api;

public interface IImportSeasonApiService
{
    Task<ApiResult<ImportSeasonResponseDto>> ImportSeasonAsync(ImportSeasonRequestDto request, CancellationToken cancellationToken = default);
}
