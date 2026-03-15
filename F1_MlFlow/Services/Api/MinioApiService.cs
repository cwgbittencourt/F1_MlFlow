using System.Text.Json;
using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Minio;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class MinioApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions, IOptions<BackendDependencySettings> backendOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IMinioApiService
{
    private const int DefaultLimit = 200;
    private readonly string? _browserBaseUrl = BuildBrowserBaseUrl(backendOptions.Value);

    public Task<ApiResult<IReadOnlyList<MinioObjectDto>>> GetObjectsAsync(int? limit = null, CancellationToken cancellationToken = default)
    {
        var effectiveLimit = limit.GetValueOrDefault(DefaultLimit);
        if (effectiveLimit <= 0)
        {
            effectiveLimit = DefaultLimit;
        }

        var uri = $"/minio/objects?limit={effectiveLimit}";
        return GetObjectsInternalAsync(uri, cancellationToken);
    }

    private async Task<ApiResult<IReadOnlyList<MinioObjectDto>>> GetObjectsInternalAsync(string uri, CancellationToken cancellationToken)
    {
        string? responseContent = null;
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<IReadOnlyList<MinioObjectDto>>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return ApiResult<IReadOnlyList<MinioObjectDto>>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            if (TryGetArray(root, out var array))
            {
                var items = array.EnumerateArray()
                    .Select(MapObject)
                    .Where(item => item is not null)
                    .Select(item => item!)
                    .ToList();

                return ApiResult<IReadOnlyList<MinioObjectDto>>.Success(items);
            }

            if (LooksLikeSingleObject(root))
            {
                var single = MapObject(root);
                return single is null
                    ? ApiResult<IReadOnlyList<MinioObjectDto>>.Failure("Formato inválido para objeto MinIO.")
                    : ApiResult<IReadOnlyList<MinioObjectDto>>.Success(new List<MinioObjectDto> { single });
            }

            var logPath = SaveRawResponse(responseContent);
            return ApiResult<IReadOnlyList<MinioObjectDto>>.Failure(
                $"Formato inesperado em {BuildEndpoint(uri)}. Esperado array ou objeto com 'objects'.{FormatLogSuffix(logPath)}");
        }
        catch (JsonException ex)
        {
            var logPath = SaveRawResponse(responseContent ?? string.Empty);
            return ApiResult<IReadOnlyList<MinioObjectDto>>.Failure(
                $"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}{FormatLogSuffix(logPath)}");
        }
        catch (Exception ex)
        {
            var logPath = SaveRawResponse(responseContent ?? string.Empty);
            return ApiResult<IReadOnlyList<MinioObjectDto>>.Failure(
                $"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}{FormatLogSuffix(logPath)}");
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
            if (TryGetPropertyIgnoreCase(root, "objects", out var objects) && objects.ValueKind == JsonValueKind.Array)
            {
                array = objects;
                return true;
            }

            if (TryGetPropertyIgnoreCase(root, "items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                array = items;
                return true;
            }

            if (TryGetPropertyIgnoreCase(root, "data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                array = data;
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

    private static bool LooksLikeSingleObject(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        return HasAnyProperty(root, "object_name", "objectName", "name", "key", "path");
    }

    private MinioObjectDto? MapObject(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var bucket = GetString(element, "bucket", "bucket_name", "bucketName");
        var prefix = GetString(element, "prefix", "path_prefix", "pathPrefix");
        var objectName = GetString(element, "object_name", "objectName", "name", "key", "object", "path");
        var storageUri = GetString(element, "storage_uri", "storageUri", "s3_uri", "s3Uri", "uri", "url", "href");

        if (string.IsNullOrWhiteSpace(storageUri) && !string.IsNullOrWhiteSpace(bucket) && !string.IsNullOrWhiteSpace(objectName))
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(prefix) ? null : prefix.Trim('/');
            var combined = string.IsNullOrWhiteSpace(normalizedPrefix)
                ? objectName.TrimStart('/')
                : $"{normalizedPrefix}/{objectName.TrimStart('/')}";
            storageUri = $"s3://{bucket}/{combined}";
        }

        storageUri = NormalizeBrowserUri(storageUri, bucket);

        return new MinioObjectDto
        {
            Bucket = bucket,
            Prefix = prefix,
            ObjectName = objectName,
            ObjectType = GetString(element, "object_type", "objectType", "type"),
            Size = GetLong(element, "size", "bytes"),
            LastModified = GetDateTimeOffset(element, "last_modified", "lastModified", "last_modified_at", "lastModifiedAt", "modified_at", "updated_at"),
            Layer = GetString(element, "layer", "tier"),
            StorageUri = storageUri
        };
    }

    private static string? BuildBrowserBaseUrl(BackendDependencySettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DataLakeS3EndpointUrl) || string.IsNullOrWhiteSpace(settings.DataLakeBucket))
        {
            return null;
        }

        if (!Uri.TryCreate(settings.DataLakeS3EndpointUrl, UriKind.Absolute, out var endpoint))
        {
            return null;
        }

        var builder = new UriBuilder(endpoint);
        if (builder.Port == 9000)
        {
            builder.Port = 9001;
        }

        var baseUrl = builder.Uri.ToString().TrimEnd('/');
        return $"{baseUrl}/browser/{settings.DataLakeBucket}";
    }

    private string? NormalizeBrowserUri(string? storageUri, string? bucket)
    {
        if (string.IsNullOrWhiteSpace(storageUri) || _browserBaseUrl is null)
        {
            return storageUri;
        }

        if (Uri.TryCreate(storageUri, UriKind.Absolute, out var absolute) && absolute.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return storageUri;
        }

        var relativePath = storageUri;
        if (storageUri.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
        {
            var withoutScheme = storageUri.Substring(5);
            var slashIndex = withoutScheme.IndexOf('/');
            if (slashIndex >= 0 && slashIndex + 1 < withoutScheme.Length)
            {
                relativePath = withoutScheme[(slashIndex + 1)..];
            }
            else
            {
                relativePath = string.Empty;
            }
        }

        if (!string.IsNullOrWhiteSpace(bucket) &&
            relativePath.StartsWith($"{bucket}/", StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath[bucket.Length..].TrimStart('/');
        }

        return $"{_browserBaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
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

    private static long? GetLong(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var result))
                {
                    return result;
                }

                if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var parsed))
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

    private string? SaveRawResponse(string content)
    {
        try
        {
            var logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(logsDir);
            var timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
            var filePath = Path.Combine(logsDir, $"minio-objects-raw-{timestamp}.json");
            var latestPath = Path.Combine(logsDir, "minio-objects-raw-latest.json");
            File.WriteAllText(filePath, content);
            File.WriteAllText(latestPath, content);
            return filePath;
        }
        catch
        {
            return null;
        }
    }

    private static string FormatLogSuffix(string? logPath)
    {
        return string.IsNullOrWhiteSpace(logPath) ? string.Empty : $" Log salvo em: {logPath}";
    }
}
