using System.Text.Json;
using F1_MlFlow.Models.Catalog;
using F1_MlFlow.Models.Common;
using F1_MlFlow.Models.Gold;
using Microsoft.Extensions.Options;

namespace F1_MlFlow.Services.Api;

public sealed class GoldLapApiService(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiOptions)
    : ApiServiceBase(httpClientFactory, apiOptions), IGoldLapApiService
{
    public Task<ApiResult<GoldLapResult>> GetLapAsync(int season, int lapNumber, string? meetingKey, string? meetingName, CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"season={season}",
            $"lap_number={lapNumber}"
        };

        if (!string.IsNullOrWhiteSpace(meetingKey))
        {
            query.Add($"meeting_key={Uri.EscapeDataString(meetingKey.Trim())}");
        }
        else if (!string.IsNullOrWhiteSpace(meetingName))
        {
            query.Add($"meeting_name={Uri.EscapeDataString(meetingName.Trim())}");
        }

        var uri = query.Count == 0 ? "/gold/lap" : $"/gold/lap?{string.Join("&", query)}";
        return GetLapInternalAsync(uri, cancellationToken);
    }

    private async Task<ApiResult<GoldLapResult>> GetLapInternalAsync(string uri, CancellationToken cancellationToken)
    {
        try
        {
            var response = await Client.GetAsync(uri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var details = await ReadErrorDetailsAsync(response, cancellationToken);
                return ApiResult<GoldLapResult>.Failure(
                    $"Erro HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) ao consultar {BuildEndpoint(uri)}.{details}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResult<GoldLapResult>.Failure("Resposta vazia da API.");
            }

            using var doc = JsonDocument.Parse(content);
            var result = ParseResult(doc.RootElement);
            if (result.Items.Count == 0)
            {
                return ApiResult<GoldLapResult>.Failure("Nenhum dado encontrado.");
            }

            return ApiResult<GoldLapResult>.Success(result);
        }
        catch (JsonException ex)
        {
            return ApiResult<GoldLapResult>.Failure($"JSON inválido em {BuildEndpoint(uri)}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<GoldLapResult>.Failure($"Falha ao consultar API {BuildEndpoint(uri)}: {ex.Message}");
        }
    }

    private static GoldLapResult ParseResult(JsonElement root)
    {
        var items = new List<GoldRowDto>();
        var maxLap = GetInt(root, "max_lap", "max_lap_number", "max_laps_completed", "maxLap");

        if (TryGetArray(root, out var array))
        {
            foreach (var item in array.EnumerateArray())
            {
                var mapped = MapGoldRow(item);
                if (mapped is not null)
                {
                    items.Add(mapped);
                }
            }
        }
        else if (LooksLikeGoldRow(root))
        {
            var mapped = MapGoldRow(root);
            if (mapped is not null)
            {
                items.Add(mapped);
            }
        }

        if (maxLap is null && items.Count > 0)
        {
            var computed = items.Max(item => item.LapNumber ?? 0);
            if (computed > 0)
            {
                maxLap = computed;
            }
        }

        return new GoldLapResult
        {
            Items = items,
            MaxLapNumber = maxLap
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
            foreach (var name in new[] { "rows", "data", "items", "results", "records", "laps", "gold" })
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

    private static bool LooksLikeGoldRow(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Object &&
               (HasAnyProperty(root, "lap_number", "lapNumber", "driver_number", "driverNumber") ||
                HasAnyProperty(root, "avg_speed", "avgSpeed", "lap_duration", "lapDuration"));
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
