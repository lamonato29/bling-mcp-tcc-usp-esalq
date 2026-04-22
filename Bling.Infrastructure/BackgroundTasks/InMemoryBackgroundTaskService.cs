using System.Collections.Concurrent;
using System.Text.Json;
using Bling.Application.Interfaces;

namespace Bling.Infrastructure.BackgroundTasks;

/// <summary>
/// Implementação em memória de IBackgroundTaskService.
/// Útil para ambientes sem PostgreSQL (desenvolvimento / standard MCP).
/// </summary>
public class InMemoryBackgroundTaskService : IBackgroundTaskService
{
    private readonly ConcurrentDictionary<string, TaskEntry> _tasks = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Task<object> StartTaskAsync(
        string operation, object parameters, int estimatedSeconds,
        string unitsDescription, int unitsCount,
        Func<string, Action<int, string, int?>, CancellationToken, Task<object>> workFunc)
    {
        var taskId = $"TASK-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        var cts = new CancellationTokenSource();
        var now = DateTime.UtcNow;

        var entry = new TaskEntry
        {
            TaskId = taskId,
            Operation = operation,
            Status = "Pending",
            Progress = 0,
            ProgressMessage = "Aguardando início...",
            ParamsJson = JsonSerializer.Serialize(parameters, _jsonOptions),
            CreatedAt = now,
            EstimatedSeconds = estimatedSeconds,
            Cts = cts
        };

        _tasks[taskId] = entry;

        // Executar em background
        _ = Task.Run(async () =>
        {
            try
            {
                entry.Status = "Running";
                entry.StartedAt = DateTime.UtcNow;
                entry.ProgressMessage = "Iniciando...";

                Action<int, string, int?> progress = (pct, msg, est) =>
                {
                    entry.Progress = pct;
                    entry.ProgressMessage = msg;
                    if (est.HasValue) entry.EstimatedSeconds = est.Value;
                };

                var result = await workFunc(taskId, progress, cts.Token);
                entry.ResultJson = JsonSerializer.Serialize(result, _jsonOptions);
                entry.Status = "Completed";
                entry.Progress = 100;
                entry.CompletedAt = DateTime.UtcNow;
            }
            catch (OperationCanceledException)
            {
                entry.Status = "Cancelled";
                entry.ProgressMessage = "Cancelado pelo usuário.";
            }
            catch (Exception ex)
            {
                entry.Status = "Failed";
                entry.ErrorMessage = ex.Message;
                entry.CompletedAt = DateTime.UtcNow;
            }
        });

        return Task.FromResult<object>(new
        {
            TaskId = taskId,
            Status = "Pending",
            Operation = operation,
            EstimatedSeconds = estimatedSeconds,
            Mensagem = $"Task iniciada (Em Memória). Use bling_task_status com taskId '{taskId}' para acompanhar."
        });
    }

    public Task<object?> GetTaskStatusAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var entry)) return Task.FromResult<object?>(null);

        var status = new
        {
            entry.TaskId,
            entry.Operation,
            entry.Status,
            entry.Progress,
            entry.ProgressMessage,
            CriadaEm = entry.CreatedAt.ToString("HH:mm:ss"),
            IniciadaEm = entry.StartedAt?.ToString("HH:mm:ss"),
            ConcluidaEm = entry.CompletedAt?.ToString("HH:mm:ss"),
            entry.EstimatedSeconds,
            Erro = entry.ErrorMessage,
            Duracao = entry.StartedAt.HasValue
                ? ((entry.CompletedAt ?? DateTime.UtcNow) - entry.StartedAt.Value).ToString(@"mm\:ss")
                : null
        };

        return Task.FromResult<object?>(status);
    }

    public Task<object?> GetTaskResultAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var entry)) return Task.FromResult<object?>(null);

        if (entry.Status != "Completed") 
            return Task.FromResult<object?>(new { erro = $"Task ainda não concluída. Status: {entry.Status}" });

        if (entry.ResultJson == null) 
            return Task.FromResult<object?>(new { erro = "Sem resultado disponível." });

        return Task.FromResult<object?>(JsonSerializer.Deserialize<object>(entry.ResultJson));
    }

    public Task<bool> CancelTaskAsync(string taskId)
    {
        if (!_tasks.TryGetValue(taskId, out var entry)) return Task.FromResult(false);
        
        if (entry.Status == "Pending" || entry.Status == "Running")
        {
            entry.Cts?.Cancel();
            entry.Status = "Cancelled";
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<object> ListRecentTasksAsync(int limit = 10)
    {
        var recentTasks = _tasks.Values
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new
            {
                t.TaskId,
                t.Operation,
                t.Status,
                t.Progress,
                t.ProgressMessage,
                CriadaEm = t.CreatedAt.ToString("HH:mm:ss")
            })
            .ToList();

        return Task.FromResult<object>(new { Total = recentTasks.Count, Tasks = recentTasks });
    }

    private class TaskEntry
    {
        public required string TaskId { get; set; }
        public required string Operation { get; set; }
        public required string Status { get; set; }
        public int Progress { get; set; }
        public string? ProgressMessage { get; set; }
        public string? ParamsJson { get; set; }
        public string? ResultJson { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int EstimatedSeconds { get; set; }
        public CancellationTokenSource? Cts { get; set; }
    }
}
