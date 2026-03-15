using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;

namespace F1_MlFlow.Services.Api;

public interface IGoldQuestionApiService
{
    Task<ApiResult<GoldQuestionResponseDto>> AskQuestionAsync(GoldQuestionRequestDto request, CancellationToken cancellationToken = default);
}
