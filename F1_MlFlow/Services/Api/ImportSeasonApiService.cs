using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Import;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class ImportSeasonApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IImportSeasonApiService
{
    public Task<ApiResult<ImportSeasonResponseDto>> ImportSeasonAsync(ImportSeasonRequestDto request, CancellationToken cancellationToken = default)
    {
        return PostAsync<ImportSeasonRequestDto, ImportSeasonResponseDto>("/import-season", request, cancellationToken);
    }
}
