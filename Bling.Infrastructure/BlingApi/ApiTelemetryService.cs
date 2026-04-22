namespace Bling.Infrastructure.BlingApi;

/// <summary>
/// Registro de uma chamada API
/// </summary>
public class ApiCallRecord
{
    public string Endpoint { get; set; } = "";
    public string Method { get; set; } = "GET";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long DurationMs => (long)(EndTime - StartTime).TotalMilliseconds;
    public bool Success { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Serviço de telemetria para rastrear chamadas API
/// </summary>
public class ApiTelemetryService
{
    private readonly List<ApiCallRecord> _records = new();
    private readonly object _lock = new();

    public int TotalCalls => _records.Count;

    public void RecordCall(ApiCallRecord record)
    {
        lock (_lock)
        {
            _records.Add(record);
            var status = record.Success ? "OK" : $"FAIL({record.StatusCode})";
            Console.Error.WriteLine($"[API] {record.Method} {record.Endpoint} - {record.DurationMs}ms [{status}]");
        }
    }

    public ApiTelemetryStats GetStats()
    {
        lock (_lock)
        {
            if (_records.Count == 0)
                return new ApiTelemetryStats();

            return new ApiTelemetryStats
            {
                TotalCalls = _records.Count,
                SuccessfulCalls = _records.Count(r => r.Success),
                FailedCalls = _records.Count(r => !r.Success),
                TotalDurationMs = _records.Sum(r => r.DurationMs),
                AverageDurationMs = (long)_records.Average(r => r.DurationMs),
            };
        }
    }

    public void Clear()
    {
        lock (_lock) { _records.Clear(); }
    }
}

public class ApiTelemetryStats
{
    public int TotalCalls { get; set; }
    public int SuccessfulCalls { get; set; }
    public int FailedCalls { get; set; }
    public long TotalDurationMs { get; set; }
    public long AverageDurationMs { get; set; }
}
