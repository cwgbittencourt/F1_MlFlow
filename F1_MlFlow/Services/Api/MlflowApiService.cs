using System.Text.Json;
using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Mlflow;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class MlflowApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IMlflowApiService
{
    private const int DefaultLimit = 50;

    public Task<ApiResult<IReadOnlyList<MlflowRunDto>>> GetRunsAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var effectiveLimit = limit.GetValueOrDefault(DefaultLimit);
        if (effectiveLimit <= 0)
        {
            effectiveLimit = DefaultLimit;
        }

        var uri = $"/mlflow/runs?limit={effectiveLimit}";
        return GetRunsInternalAsync(uri, cancellationToken);
    }

    private async Task<ApiResult<IReadOnlyList<MlflowRunDto>>> GetRunsInternalAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<IReadOnlyList<MlflowRunDto>>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<IReadOnlyList<MlflowRunDto>>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (TryGetArray(root, out var array))
            {
                var runs = array.EnumerateArray()
                    .Select(MapRun)
                    .Where(run => run is not null)
                    .Select(run => run!)
                    .ToList();

                return ApiResult<IReadOnlyList<MlflowRunDto>>.Success(runs);
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                var single = MapRun(root);
                return single is null
                    ? ApiResult<IReadOnlyList<MlflowRunDto>>.Failure("Formato inválido para run.")
                    : ApiResult<IReadOnlyList<MlflowRunDto>>.Success(new List<MlflowRunDto> { single });
            }

            return ApiResult<IReadOnlyList<MlflowRunDto>>.Failure(
                $"Formato inesperado em {BuildEndpoint(uri)}. Esperado array ou objeto com 'runs'.");
        }
        catch (JsonException ex)
        {
            return ApiResult<IReadOnlyList<MlflowRunDto>>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<MlflowRunDto>>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private static bool TryGetArray(JsonElement root, out JsonElement array)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            array = root;
            return true;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyIgnoreCase(root, "runs", out var runs) && runs.ValueKind == JsonValueKind.Array)
            {
                array = runs;
                return true;
            }

            if (TryGetPropertyIgnoreCase(root, "data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                array = data;
                return true;
            }

            if (TryGetPropertyIgnoreCase(root, "items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                array = items;
                return true;
            }

            if (TryGetPropertyIgnoreCase(root, "results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                array = results;
                return true;
            }
        }

        array = default;
        return false;
    }

    private static MlflowRunDto? MapRun(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var runId = GetString(element, "run_id", "runId", "run_uuid", "runUuid", "id");
        if (string.IsNullOrWhiteSpace(runId))
        {
            return null;
        }

        var startTime = GetDateTimeOffset(element, "start_time", "startTime", "started_at", "startedAt");
        var endTime = GetDateTimeOffset(element, "end_time", "endTime", "finished_at", "finishedAt");
        double? duration = GetDouble(element, "duration", "duration_sec", "duration_seconds");
        if (duration is null && startTime is not null && endTime is not null)
        {
            duration = (endTime.Value - startTime.Value).TotalSeconds;
        }

        var metrics = ParseMetrics(element);
        var parameters = ParseParameters(element);

        return new MlflowRunDto
        {
            ExperimentName = GetString(element, "experiment_name", "experimentName", "experiment"),
            RunId = runId,
            RunName = GetString(element, "run_name", "runName") ?? runId,
            Status = GetString(element, "status", "state"),
            StartTime = startTime,
            EndTime = endTime,
            Duration = duration,
            Metrics = metrics,
            Parameters = parameters,
            Artifacts = new List<MlflowArtifactDto>()
        };
    }

    private static List<MlflowMetricDto> ParseMetrics(JsonElement element)
    {
        if (!TryGetPropertyIgnoreCase(element, "metrics", out var metricsElement))
        {
            return new List<MlflowMetricDto>();
        }

        if (metricsElement.ValueKind == JsonValueKind.Array)
        {
            return metricsElement.EnumerateArray()
                .Select(metric => new MlflowMetricDto
                {
                    Key = GetString(metric, "key", "name") ?? string.Empty,
                    Value = GetDouble(metric, "value")
                })
                .Where(metric => !string.IsNullOrWhiteSpace(metric.Key))
                .ToList();
        }

        if (metricsElement.ValueKind == JsonValueKind.Object)
        {
            return metricsElement.EnumerateObject()
                .Select(entry => new MlflowMetricDto
                {
                    Key = entry.Name,
                    Value = entry.Value.ValueKind == JsonValueKind.Number && entry.Value.TryGetDouble(out var numeric)
                        ? numeric
                        : (double?)null
                })
                .ToList();
        }

        return new List<MlflowMetricDto>();
    }

    private static List<MlflowParameterDto> ParseParameters(JsonElement element)
    {
        if (!TryGetPropertyIgnoreCase(element, "parameters", out var paramsElement) &&
            !TryGetPropertyIgnoreCase(element, "params", out paramsElement))
        {
            return new List<MlflowParameterDto>();
        }

        if (paramsElement.ValueKind == JsonValueKind.Array)
        {
            return paramsElement.EnumerateArray()
                .Select(param => new MlflowParameterDto
                {
                    Key = GetString(param, "key", "name") ?? string.Empty,
                    Value = GetString(param, "value")
                })
                .Where(param => !string.IsNullOrWhiteSpace(param.Key))
                .ToList();
        }

        if (paramsElement.ValueKind == JsonValueKind.Object)
        {
            return paramsElement.EnumerateObject()
                .Select(entry => new MlflowParameterDto
                {
                    Key = entry.Name,
                    Value = entry.Value.ToString()
                })
                .ToList();
        }

        return new List<MlflowParameterDto>();
    }

    private static string? GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }

                if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                {
                    return value.ToString();
                }
            }
        }

        return null;
    }

    private static double? GetDouble(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var result))
                {
                    return result;
                }

                if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), out var parsed))
                {
                    return parsed;
                }
            }
        }

        return null;
    }

    private static DateTimeOffset? GetDateTimeOffset(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetPropertyIgnoreCase(element, name, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                if (DateTimeOffset.TryParse(value.GetString(), out var parsed))
                {
                    return parsed;
                }
            }
            else if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var numeric))
            {
                if (numeric > 1_000_000_000_000)
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(numeric);
                }

                if (numeric > 1_000_000_000)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(numeric);
                }
            }
        }

        return null;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static async Task<string> ReadErrorDetailsAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var trimmed = content.Trim();
            if (trimmed.Length > 200)
            {
                trimmed = $"{trimmed[..200]}...";
            }

            return $" Detalhe: {trimmed}";
        }
        catch
        {
            return string.Empty;
        }
    }

    private string BuildEndpoint(string uri)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (Client.BaseAddress is null)
        {
            return uri;
        }

        return new Uri(Client.BaseAddress, uri).ToString();
    }
}
