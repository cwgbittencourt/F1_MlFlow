using System.Text.Json;
using F1_MlFlow.Models.Catalog;
using F1_MlFlow.Models.Common;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class DataCatalogApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IDataCatalogApiService
{
    public Task<ApiResult<IReadOnlyList<BronzeRowDto>>> GetBronzeAsync(bool checkSync = false, int? limit = null, int? season = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (limit is not null && limit > 0)
        {
            query.Add($"limit={limit}");
        }

        if (season is not null && season > 0)
        {
            query.Add($"season={season}");
        }

        query.Add($"check_sync={checkSync.ToString().ToLowerInvariant()}");
        var uri = BuildUri("/catalog/bronze", query);
        return GetCatalogAsync(uri, MapBronzeRow, cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<SilverRowDto>>> GetSilverAsync(int? limit = null, int? season = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (limit is not null && limit > 0)
        {
            query.Add($"limit={limit}");
        }

        if (season is not null && season > 0)
        {
            query.Add($"season={season}");
        }

        var uri = BuildUri("/catalog/silver", query);
        return GetCatalogAsync(uri, MapSilverRow, cancellationToken);
    }

    public Task<ApiResult<IReadOnlyList<GoldRowDto>>> GetGoldAsync(bool includeSchema = false, int? limit = null, int? season = null, CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (includeSchema)
        {
            query.Add("include_schema=true");
        }

        if (limit is not null && limit > 0)
        {
            query.Add($"limit={limit}");
        }

        if (season is not null && season > 0)
        {
            query.Add($"season={season}");
        }

        var uri = BuildUri("/catalog/gold", query);
        return GetCatalogAsync(uri, MapGoldRow, cancellationToken);
    }

    private async Task<ApiResult<IReadOnlyList<T>>> GetCatalogAsync<T>(string uri, Func<JsonElement, T?> mapper, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<IReadOnlyList<T>>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<IReadOnlyList<T>>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            if (TryGetArray(root, out var array))
            {
                var items = array.EnumerateArray()
                    .Select(mapper)
                    .Where(item => item is not null)
                    .Select(item => item!)
                    .ToList();

                return ApiResult<IReadOnlyList<T>>.Success(items);
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                var single = mapper(root);
                return single is null
                    ? ApiResult<IReadOnlyList<T>>.Failure("Formato inválido para catálogo.")
                    : ApiResult<IReadOnlyList<T>>.Success(new List<T> { single });
            }

            return ApiResult<IReadOnlyList<T>>.Failure(
                $"Formato inesperado em {BuildEndpoint(uri)}. Esperado array ou objeto com 'data/rows/items'.");
        }
        catch (JsonException ex)
        {
            return ApiResult<IReadOnlyList<T>>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<T>>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private static BronzeRowDto? MapBronzeRow(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new BronzeRowDto
        {
            Season = GetInt(element, "season"),
            MeetingKey = GetString(element, "meeting_key", "meetingKey", "meeting"),
            SessionKey = GetString(element, "session_key", "sessionKey", "session"),
            DriverNumber = GetInt(element, "driver_number", "driverNumber", "driver"),
            SourceEndpoint = GetString(element, "source_endpoint", "sourceEndpoint", "endpoint"),
            FileName = GetString(element, "file_name", "fileName", "filename"),
            CreatedAt = GetDateTimeOffset(element, "created_at", "createdAt", "created"),
            RecordCount = GetInt(element, "record_count", "recordCount", "rows", "count"),
            StoragePath = GetString(element, "storage_path", "storagePath", "path", "s3_path"),
            SyncStatus = GetString(element, "sync_status", "syncStatus", "sync")
        };
    }

    private static SilverRowDto? MapSilverRow(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new SilverRowDto
        {
            Season = GetInt(element, "season"),
            MeetingKey = GetString(element, "meeting_key", "meetingKey", "meeting"),
            SessionKey = GetString(element, "session_key", "sessionKey", "session"),
            DriverNumber = GetInt(element, "driver_number", "driverNumber", "driver"),
            DatasetName = GetString(element, "dataset_name", "datasetName", "dataset"),
            NormalizedAt = GetDateTimeOffset(element, "normalized_at", "normalizedAt", "normalized"),
            SchemaVersion = GetString(element, "schema_version", "schemaVersion", "schema"),
            RecordCount = GetInt(element, "record_count", "recordCount", "rows", "count"),
            NullPct = GetDouble(element, "null_pct", "nullPct", "null_percent", "nullPercent"),
            StoragePath = GetString(element, "storage_path", "storagePath", "path", "s3_path")
        };
    }

    private static GoldRowDto? MapGoldRow(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new GoldRowDto
        {
            Season = GetInt(element, "season"),
            MeetingKey = GetString(element, "meeting_key", "meetingKey", "meeting"),
            MeetingName = GetString(element, "meeting_name", "meetingName", "meeting"),
            MeetingDateStart = GetDateTimeOffset(element, "meeting_date_start", "meetingDateStart", "meeting_start_date", "meetingStartDate", "meeting_start"),
            SessionKey = GetString(element, "session_key", "sessionKey", "session"),
            SessionName = GetString(element, "session_name", "sessionName", "session"),
            DriverNumber = GetInt(element, "driver_number", "driverNumber", "driver"),
            DriverName = GetString(element, "driver_name", "driverName", "driver"),
            TeamName = GetString(element, "team_name", "teamName", "team"),
            LapNumber = GetInt(element, "lap_number", "lapNumber", "lap"),
            LapDuration = GetDouble(element, "lap_duration", "lapDuration", "lap_time", "lapTime"),
            DurationSector1 = GetDouble(element, "duration_sector1", "durationSector1", "sector1"),
            DurationSector2 = GetDouble(element, "duration_sector2", "durationSector2", "sector2"),
            DurationSector3 = GetDouble(element, "duration_sector3", "durationSector3", "sector3"),
            AvgSpeed = GetDouble(element, "avg_speed", "avgSpeed", "speed_avg", "speedAvg"),
            MaxSpeed = GetDouble(element, "max_speed", "maxSpeed"),
            MinSpeed = GetDouble(element, "min_speed", "minSpeed"),
            SpeedStd = GetDouble(element, "speed_std", "speedStd", "speed_stdev", "speedStdev"),
            AvgRpm = GetDouble(element, "avg_rpm", "avgRpm", "rpm_avg", "rpmAvg"),
            MaxRpm = GetDouble(element, "max_rpm", "maxRpm"),
            MinRpm = GetDouble(element, "min_rpm", "minRpm"),
            RpmStd = GetDouble(element, "rpm_std", "rpmStd", "rpm_stdev", "rpmStdev"),
            AvgThrottle = GetDouble(element, "avg_throttle", "avgThrottle", "throttle_avg", "throttleAvg"),
            MaxThrottle = GetDouble(element, "max_throttle", "maxThrottle"),
            MinThrottle = GetDouble(element, "min_throttle", "minThrottle"),
            ThrottleStd = GetDouble(element, "throttle_std", "throttleStd", "throttle_stdev", "throttleStdev"),
            FullThrottlePct = GetDouble(element, "full_throttle_pct", "fullThrottlePct", "full_throttle_percent", "fullThrottlePercent"),
            BrakePct = GetDouble(element, "brake_pct", "brakePct", "brake_percent", "brakePercent"),
            BrakeEvents = GetInt(element, "brake_events", "brakeEvents"),
            HardBrakeEvents = GetInt(element, "hard_brake_events", "hardBrakeEvents"),
            DrsPct = GetDouble(element, "drs_pct", "drsPct", "drs_percent", "drsPercent"),
            GearChanges = GetInt(element, "gear_changes", "gearChanges"),
            DistanceTraveled = GetDouble(element, "distance_traveled", "distanceTraveled", "distance"),
            TrajectoryLength = GetDouble(element, "trajectory_length", "trajectoryLength"),
            TrajectoryVariation = GetDouble(element, "trajectory_variation", "trajectoryVariation"),
            TelemetryPoints = GetInt(element, "telemetry_points", "telemetryPoints"),
            TrajectoryPoints = GetInt(element, "trajectory_points", "trajectoryPoints"),
            HasTelemetry = GetBool(element, "has_telemetry", "hasTelemetry", "telemetry"),
            HasTrajectory = GetBool(element, "has_trajectory", "hasTrajectory", "trajectory"),
            StintNumber = GetInt(element, "stint_number", "stintNumber"),
            Compound = GetString(element, "compound", "tyre_compound", "tyreCompound", "tire_compound", "tireCompound"),
            StintLapStart = GetInt(element, "stint_lap_start", "stintLapStart"),
            StintLapEnd = GetInt(element, "stint_lap_end", "stintLapEnd"),
            TyreAgeAtStart = GetInt(element, "tyre_age_at_start", "tyreAgeAtStart", "tire_age_at_start", "tireAgeAtStart"),
            TyreAgeAtLap = GetInt(element, "tyre_age_at_lap", "tyreAgeAtLap", "tire_age_at_lap", "tireAgeAtLap"),
            TrackTemperature = GetDouble(element, "track_temperature", "trackTemperature", "track_temp", "trackTemp"),
            AirTemperature = GetDouble(element, "air_temperature", "airTemperature", "air_temp", "airTemp"),
            WeatherDate = GetDateTimeOffset(element, "weather_date", "weatherDate", "weather_time", "weatherTime"),
            IsPitOutLap = GetBool(element, "is_pit_out_lap", "isPitOutLap", "pit_out_lap", "pitOutLap"),
            StoragePath = GetString(element, "storage_path", "storagePath", "path", "s3_path")
        };
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
            foreach (var name in new[] { "rows", "data", "items", "results", "records", "bronze", "silver", "gold" })
            {
                if (TryGetPropertyIgnoreCase(root, name, out var value))
                {
                    if (value.ValueKind == JsonValueKind.Array)
                    {
                        array = value;
                        return true;
                    }

                    if (value.ValueKind == JsonValueKind.Object && TryGetArray(value, out array))
                    {
                        return true;
                    }
                }
            }
        }

        array = default;
        return false;
    }

    private static string BuildUri(string path, List<string> query)
    {
        return query.Count == 0 ? path : $"{path}?{string.Join("&", query)}";
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

    private static int? GetInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                {
                    if (value.TryGetInt32(out var result))
                    {
                        return result;
                    }

                    if (value.TryGetInt64(out var longResult))
                    {
                        return (int)longResult;
                    }
                }

                if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
                {
                    return parsed;
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

    private static bool? GetBool(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetPropertyIgnoreCase(element, name, out var value))
            {
                if (value.ValueKind == JsonValueKind.True)
                {
                    return true;
                }

                if (value.ValueKind == JsonValueKind.False)
                {
                    return false;
                }

                if (value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed))
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
