using System.Text.Json;
using Bling.Application.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace Bling.Infrastructure.BackgroundTasks;

/// <summary>
/// Implementação de IBackgroundTaskService usando PostgreSQL.
/// Persiste status e resultados de tarefas por 24 horas.
/// </summary>
public class PostgresBackgroundTaskService : IBackgroundTaskService
{
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PostgresBackgroundTaskService(string connectionString)
    {
        _connectionString = connectionString;
        InitializeTable();
    }

    public async Task<object> StartTaskAsync(
        string operation, object parameters, int estimatedSeconds,
        string unitsDescription, int unitsCount,
        Func<string, Action<int, string, int?>, CancellationToken, Task<object>> workFunc)
    {
        var taskId = GenerateTaskId();
        var cts = new CancellationTokenSource();
        var now = DateTime.UtcNow;

        // Salvar task como Pending
        await SaveTaskAsync(taskId, operation, "Pending", 0, "Aguardando início...",
            JsonSerializer.Serialize(parameters, _jsonOptions), null, null, now, null, null, estimatedSeconds);

        // Executar em background
        _ = Task.Run(async () =>
        {
            try
            {
                await UpdateStatusAsync(taskId, "Running", 0, "Iniciando...", now);

                Action<int, string, int?> progress = (pct, msg, est) =>
                {
                    _ = UpdateProgressAsync(taskId, pct, msg, est);
                };

                var result = await workFunc(taskId, progress, cts.Token);
                var resultJson = JsonSerializer.Serialize(result, _jsonOptions);
                await CompleteAsync(taskId, resultJson);
            }
            catch (OperationCanceledException)
            {
                await UpdateStatusAsync(taskId, "Cancelled", 0, "Cancelado pelo usuário.", null);
            }
            catch (Exception ex)
            {
                await FailAsync(taskId, ex.Message);
            }
        });

        return new
        {
            TaskId = taskId,
            Status = "Pending",
            Operation = operation,
            EstimatedSeconds = estimatedSeconds,
            Mensagem = $"Task iniciada. Use bling_task_status com taskId '{taskId}' para acompanhar."
        };
    }

    public async Task<object?> GetTaskStatusAsync(string taskId)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "SELECT task_id, operation, status, progress, progress_message, " +
                "created_at, started_at, completed_at, estimated_seconds, error_message " +
                "FROM mcp_background_tasks WHERE task_id = @id", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            var createdAt = reader.GetDateTime(5);
            var startedAt = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
            var completedAt = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7);

            return new
            {
                TaskId = reader.GetString(0),
                Operation = reader.GetString(1),
                Status = reader.GetString(2),
                Progress = reader.GetInt32(3),
                ProgressMessage = reader.IsDBNull(4) ? null : reader.GetString(4),
                CriadaEm = createdAt.ToString("HH:mm:ss"),
                IniciadaEm = startedAt?.ToString("HH:mm:ss"),
                ConcluidaEm = completedAt?.ToString("HH:mm:ss"),
                EstimatedSeconds = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                Erro = reader.IsDBNull(9) ? null : reader.GetString(9),
                Duracao = startedAt.HasValue
                    ? ((completedAt ?? DateTime.UtcNow) - startedAt.Value).ToString(@"mm\:ss")
                    : null
            };
        }
        catch { return null; }
    }

    public async Task<object?> GetTaskResultAsync(string taskId)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "SELECT result_json, status FROM mcp_background_tasks WHERE task_id = @id", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;

            var status = reader.GetString(1);
            if (status != "Completed") return new { erro = $"Task ainda não concluída. Status: {status}" };

            var json = reader.IsDBNull(0) ? null : reader.GetString(0);
            if (json == null) return new { erro = "Sem resultado disponível." };

            return JsonSerializer.Deserialize<object>(json);
        }
        catch { return null; }
    }

    public async Task<bool> CancelTaskAsync(string taskId)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE mcp_background_tasks SET status = 'Cancelled' " +
                "WHERE task_id = @id AND status = 'Pending'", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        catch { return false; }
    }

    public async Task<object> ListRecentTasksAsync(int limit = 10)
    {
        var tasks = new List<object>();
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "SELECT task_id, operation, status, progress, progress_message, created_at " +
                "FROM mcp_background_tasks WHERE created_at > @since " +
                "ORDER BY created_at DESC LIMIT @limit", conn);
            cmd.Parameters.AddWithValue("since", DateTime.UtcNow.AddHours(-24));
            cmd.Parameters.AddWithValue("limit", limit);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(new
                {
                    TaskId = reader.GetString(0),
                    Operation = reader.GetString(1),
                    Status = reader.GetString(2),
                    Progress = reader.GetInt32(3),
                    ProgressMessage = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CriadaEm = reader.GetDateTime(5).ToString("HH:mm:ss")
                });
            }
        }
        catch { }
        return new { Total = tasks.Count, Tasks = tasks };
    }

    #region Private Helpers

    private async Task SaveTaskAsync(string taskId, string operation, string status,
        int progress, string? progressMessage, string? paramsJson, string? resultJson,
        string? errorMessage, DateTime createdAt, DateTime? startedAt, DateTime? completedAt,
        int estimatedSeconds)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(@"
                INSERT INTO mcp_background_tasks 
                (task_id, operation, status, progress, progress_message, params_json, 
                 result_json, error_message, created_at, started_at, completed_at, estimated_seconds)
                VALUES (@id, @op, @status, @progress, @msg, @params, @result, @error, 
                        @created, @started, @completed, @est)", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            cmd.Parameters.AddWithValue("op", operation);
            cmd.Parameters.AddWithValue("status", status);
            cmd.Parameters.AddWithValue("progress", progress);
            cmd.Parameters.AddWithValue("msg", (object?)progressMessage ?? DBNull.Value);
            cmd.Parameters.Add(new NpgsqlParameter("params", NpgsqlDbType.Jsonb) { Value = (object?)paramsJson ?? DBNull.Value });
            cmd.Parameters.Add(new NpgsqlParameter("result", NpgsqlDbType.Jsonb) { Value = (object?)resultJson ?? DBNull.Value });
            cmd.Parameters.AddWithValue("error", (object?)errorMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("created", createdAt);
            cmd.Parameters.AddWithValue("started", (object?)startedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("completed", (object?)completedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("est", estimatedSeconds);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[BackgroundTask] Save error: {ex.Message}"); }
    }

    private async Task UpdateStatusAsync(string taskId, string status, int progress, string? message, DateTime? startedAt)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = startedAt.HasValue
                ? "UPDATE mcp_background_tasks SET status=@s, progress=@p, progress_message=@m, started_at=@st WHERE task_id=@id"
                : "UPDATE mcp_background_tasks SET status=@s, progress=@p, progress_message=@m WHERE task_id=@id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", taskId);
            cmd.Parameters.AddWithValue("s", status);
            cmd.Parameters.AddWithValue("p", progress);
            cmd.Parameters.AddWithValue("m", (object?)message ?? DBNull.Value);
            if (startedAt.HasValue) cmd.Parameters.AddWithValue("st", startedAt.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    private async Task UpdateProgressAsync(string taskId, int progress, string? message, int? estimatedSeconds = null)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = estimatedSeconds.HasValue
                ? "UPDATE mcp_background_tasks SET progress=@p, progress_message=@m, estimated_seconds=@est WHERE task_id=@id"
                : "UPDATE mcp_background_tasks SET progress=@p, progress_message=@m WHERE task_id=@id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", taskId);
            cmd.Parameters.AddWithValue("p", progress);
            cmd.Parameters.AddWithValue("m", (object?)message ?? DBNull.Value);
            if (estimatedSeconds.HasValue) cmd.Parameters.AddWithValue("est", estimatedSeconds.Value);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    private async Task CompleteAsync(string taskId, string resultJson)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE mcp_background_tasks SET status='Completed', progress=100, " +
                "result_json=@result, completed_at=@now WHERE task_id=@id", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            cmd.Parameters.Add(new NpgsqlParameter("result", NpgsqlDbType.Jsonb) { Value = resultJson });
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    private async Task FailAsync(string taskId, string errorMessage)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand(
                "UPDATE mcp_background_tasks SET status='Failed', error_message=@err, " +
                "completed_at=@now WHERE task_id=@id", conn);
            cmd.Parameters.AddWithValue("id", taskId);
            cmd.Parameters.AddWithValue("err", errorMessage);
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync();
        }
        catch { }
    }

    private static string GenerateTaskId() =>
        $"TASK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    private void InitializeTable()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS mcp_background_tasks (
                    task_id TEXT PRIMARY KEY,
                    operation TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'Pending',
                    progress INT NOT NULL DEFAULT 0,
                    progress_message TEXT,
                    params_json JSONB,
                    result_json JSONB,
                    error_message TEXT,
                    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                    started_at TIMESTAMP,
                    completed_at TIMESTAMP,
                    estimated_seconds INT DEFAULT 0
                );
                CREATE INDEX IF NOT EXISTS idx_bg_tasks_status ON mcp_background_tasks (status);
                CREATE INDEX IF NOT EXISTS idx_bg_tasks_created ON mcp_background_tasks (created_at);", conn);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { Console.Error.WriteLine($"[BackgroundTask] Init error: {ex.Message}"); }
    }

    #endregion
}
