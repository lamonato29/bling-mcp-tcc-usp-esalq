namespace Bling.Application.Interfaces;

/// <summary>
/// Port para serviço de tarefas em background
/// </summary>
public interface IBackgroundTaskService
{
    /// <summary>
    /// Inicia uma tarefa em background
    /// </summary>
    Task<object> StartTaskAsync(
        string operation,
        object parameters,
        int estimatedSeconds,
        string unitsDescription,
        int unitsCount,
        Func<string, Action<int, string, int?>, CancellationToken, Task<object>> workFunc);

    /// <summary>
    /// Obtém o status de uma tarefa
    /// </summary>
    Task<object?> GetTaskStatusAsync(string taskId);

    /// <summary>
    /// Obtém o resultado de uma tarefa concluída
    /// </summary>
    Task<object?> GetTaskResultAsync(string taskId);

    /// <summary>
    /// Cancela uma tarefa em execução
    /// </summary>
    Task<bool> CancelTaskAsync(string taskId);

    /// <summary>
    /// Lista tarefas recentes
    /// </summary>
    Task<object> ListRecentTasksAsync(int limit = 10);
}
