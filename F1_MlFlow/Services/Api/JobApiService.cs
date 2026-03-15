using System.Text.Json;
using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Jobs;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class JobApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IJobApiService
{
    public Task<ApiResult<IReadOnlyList<JobSummaryDto>>> GetRecentJobsAsync(CancellationToken cancellationToken = default)
    {
        return GetJobsAsync("/jobs", cancellationToken);
    }

    public Task<ApiResult<JobDetailsDto>> GetJobDetailsAsync(string jobId, CancellationToken cancellationToken = default)
    {
        return GetJobDetailsInternalAsync($"/jobs/{jobId}", cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<JobLogLineDto>>> GetJobLogsAsync(string jobId, int lines = 200, CancellationToken cancellationToken = default)
    {
        return GetJobLogsInternalAsync($"/jobs/{jobId}/logs?lines={lines}", cancellationToken);
    }

    private async Task<ApiResult<IReadOnlyList<JobSummaryDto>>> GetJobsAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<IReadOnlyList<JobSummaryDto>>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<IReadOnlyList<JobSummaryDto>>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (TryGetArray(root, out var array))
            {
                var jobs = array.EnumerateArray()
                    .Select(MapJobSummary)
                    .Where(job => job is not null)
                    .Select(job => job!)
                    .ToList();

                return ApiResult<IReadOnlyList<JobSummaryDto>>.Success(jobs);
            }

            if (LooksLikeSingleJob(root))
            {
                var single = MapJobSummary(root);
                return single is null
                    ? ApiResult<IReadOnlyList<JobSummaryDto>>.Failure("Formato inválido para job.")
                    : ApiResult<IReadOnlyList<JobSummaryDto>>.Success(new List<JobSummaryDto> { single });
            }

            return ApiResult<IReadOnlyList<JobSummaryDto>>.Failure(
                $"Formato inesperado em {BuildEndpoint(uri)}. Esperado array ou objeto com 'jobs'.");
        }
        catch (JsonException ex)
        {
            return ApiResult<IReadOnlyList<JobSummaryDto>>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<JobSummaryDto>>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private async Task<ApiResult<JobDetailsDto>> GetJobDetailsInternalAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<JobDetailsDto>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<JobDetailsDto>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var first = doc.RootElement.EnumerateArray().FirstOrDefault();
                if (first.ValueKind != JsonValueKind.Undefined)
                {
                    var mapped = MapJobDetails(first);
                    return mapped is null
                        ? ApiResult<JobDetailsDto>.Failure("Formato inválido para detalhes do job.")
                        : ApiResult<JobDetailsDto>.Success(mapped);
                }
            }

            var detailsDto = MapJobDetails(doc.RootElement);
            return detailsDto is null
                ? ApiResult<JobDetailsDto>.Failure("Formato inválido para detalhes do job.")
                : ApiResult<JobDetailsDto>.Success(detailsDto);
        }
        catch (JsonException ex)
        {
            return ApiResult<JobDetailsDto>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<JobDetailsDto>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private async Task<ApiResult<IReadOnlyList<JobLogLineDto>>> GetJobLogsInternalAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<IReadOnlyList<JobLogLineDto>>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<IReadOnlyList<JobLogLineDto>>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (TryGetArray(root, out var array))
            {
                var lines = array.EnumerateArray()
                    .Select(MapLogLine)
                    .Where(line => line is not null)
                    .Select(line => line!)
                    .ToList();

                return ApiResult<IReadOnlyList<JobLogLineDto>>.Success(lines);
            }

            if (root.ValueKind == JsonValueKind.String)
            {
                return ApiResult<IReadOnlyList<JobLogLineDto>>.Success(new List<JobLogLineDto>
                {
                    new() { Line = root.GetString() }
                });
            }

            return ApiResult<IReadOnlyList<JobLogLineDto>>.Failure(
                $"Formato inesperado em {BuildEndpoint(uri)}. Esperado array de linhas.");
        }
        catch (JsonException ex)
        {
            return ApiResult<IReadOnlyList<JobLogLineDto>>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<JobLogLineDto>>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
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
            if (TryGetPropertyIgnoreCase(root, "jobs", out var jobs) && jobs.ValueKind == JsonValueKind.Array)
            {
                array = jobs;
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

            if (TryGetPropertyIgnoreCase(root, "jobs", out var jobsObject) && jobsObject.ValueKind == JsonValueKind.Object)
            {
                if (TryGetPropertyIgnoreCase(jobsObject, "items", out var nestedItems) && nestedItems.ValueKind == JsonValueKind.Array)
                {
                    array = nestedItems;
                    return true;
                }
            }
        }

        array = default;
        return false;
    }

    private static bool LooksLikeSingleJob(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return HasAnyProperty(root, "job_id", "jobId", "id");
    }

    private static JobSummaryDto? MapJobSummary(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var jobId = GetString(element, "job_id", "jobId", "id");
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        var startedAt = GetDateTimeOffset(element, "started_at", "startedAt", "started");
        var finishedAt = GetDateTimeOffset(element, "finished_at", "finishedAt", "finished", "ended_at", "endedAt");
        var duration = GetDouble(element, "duration", "duration_sec", "duration_seconds");

        if (duration is null && startedAt is not null && finishedAt is not null)
        {
            duration = (finishedAt.Value - startedAt.Value).TotalSeconds;
        }

        return new JobSummaryDto
        {
            JobId = jobId,
            JobType = GetString(element, "job_type", "jobType", "type"),
            Status = GetString(element, "status", "state"),
            CreatedAt = GetDateTimeOffset(element, "created_at", "createdAt", "created"),
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            Duration = duration,
            LastMessage = GetString(element, "last_message", "lastMessage", "message")
        };
    }

    private static JobDetailsDto? MapJobDetails(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var jobId = GetString(element, "job_id", "jobId", "id");
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        var startedAt = GetDateTimeOffset(element, "started_at", "startedAt", "started");
        var finishedAt = GetDateTimeOffset(element, "finished_at", "finishedAt", "finished", "ended_at", "endedAt");
        var duration = GetDouble(element, "duration", "duration_sec", "duration_seconds");

        if (duration is null && startedAt is not null && finishedAt is not null)
        {
            duration = (finishedAt.Value - startedAt.Value).TotalSeconds;
        }

        return new JobDetailsDto
        {
            JobId = jobId,
            JobType = GetString(element, "job_type", "jobType", "type"),
            Status = GetString(element, "status", "state"),
            CreatedAt = GetDateTimeOffset(element, "created_at", "createdAt", "created"),
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            Duration = duration,
            Params = GetString(element, "params", "parameters"),
            Filters = GetString(element, "filters", "filter"),
            LogFile = GetString(element, "log_file", "logFile"),
            StatusFile = GetString(element, "status_file", "statusFile"),
            LastMessage = GetString(element, "last_message", "lastMessage", "message")
        };
    }

    private static JobLogLineDto? MapLogLine(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return new JobLogLineDto { Line = element.GetString() };
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var line = GetString(element, "line", "message", "text");
        return string.IsNullOrWhiteSpace(line) ? null : new JobLogLineDto { Line = line };
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

                if (value.ValueKind == JsonValueKind.Number)
                {
                    return value.GetRawText();
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
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool HasAnyProperty(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out _))
            {
                return true;
            }
        }

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
