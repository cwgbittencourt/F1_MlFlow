using F1_MlFlow.Models.Catalog;
using F1_MlFlow.Models.Common;

namespace F1_MlFlow.Services.Api;

public interface IDataCatalogApiService
{
    Task<ApiResult<IReadOnlyList<BronzeRowDto>>> GetBronzeAsync(bool checkSync = false, int? limit = null, int? season = null, CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<SilverRowDto>>> GetSilverAsync(int? limit = null, int? season = null, CancellationToken cancellationToken = default);
    Task<ApiResult<IReadOnlyList<GoldRowDto>>> GetGoldAsync(bool includeSchema = false, int? limit = null, int? season = null, CancellationToken cancellationToken = default);
}
