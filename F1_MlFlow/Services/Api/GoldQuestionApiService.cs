using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class GoldQuestionApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IGoldQuestionApiService
{
    public Task<ApiResult<GoldQuestionResponseDto>> AskQuestionAsync(GoldQuestionRequestDto request, CancellationToken cancellationToken = default)
    {
        return PostAsync<GoldQuestionRequestDto, GoldQuestionResponseDto>("/gold/questions", request, cancellationToken);
    }
}
