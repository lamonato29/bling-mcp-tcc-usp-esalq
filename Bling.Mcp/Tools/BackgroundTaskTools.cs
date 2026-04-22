using System.ComponentModel;
using System.Text.Json;
using Bling.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para gerenciamento de tarefas de background.
/// </summary>
[McpServerToolType]
public static class BackgroundTaskTools
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "bling_task_status"), Description(
        "Consulta o status e progresso de uma tarefa de background. " +
        "Retorna taskId, status (Pending/Running/Completed/Failed/Cancelled), progresso e duração.")]
    public static async Task<string> GetTaskStatus(
        IServiceProvider serviceProvider,
        [Description("ID da task (ex: TASK-A1B2C3D4).")] string taskId)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (string.IsNullOrWhiteSpace(taskId))
            return JsonSerializer.Serialize(new { erro = "ID da task não informado." });

        var status = await taskService.GetTaskStatusAsync(taskId.Trim());

        if (status == null)
            return JsonSerializer.Serialize(new { erro = $"Task '{taskId}' não encontrada." });

        return JsonSerializer.Serialize(status, _jsonOptions);
    }

    [McpServerTool(Name = "bling_task_result"), Description(
        "Obtém o resultado de uma tarefa de background CONCLUÍDA. " +
        "A task deve estar com status Completed para ter resultado.")]
    public static async Task<string> GetTaskResult(
        IServiceProvider serviceProvider,
        [Description("ID da task.")] string taskId)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (string.IsNullOrWhiteSpace(taskId))
            return JsonSerializer.Serialize(new { erro = "ID da task não informado." });

        var result = await taskService.GetTaskResultAsync(taskId.Trim());

        if (result == null)
            return JsonSerializer.Serialize(new { erro = $"Resultado da task '{taskId}' não disponível." });

        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "bling_task_list"), Description(
        "Lista as tarefas de background recentes (últimas 24 horas). " +
        "Retorna ID, operação, status e progresso de cada task.")]
    public static async Task<string> ListTasks(
        IServiceProvider serviceProvider,
        [Description("Limite de resultados. Padrão: 10.")] int limite = 10)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        var tasks = await taskService.ListRecentTasksAsync(Math.Min(limite, 50));
        return JsonSerializer.Serialize(tasks, _jsonOptions);
    }

    [McpServerTool(Name = "bling_task_cancel"), Description(
        "Cancela uma tarefa de background pendente. " +
        "Só é possível cancelar tasks com status Pending.")]
    public static async Task<string> CancelTask(
        IServiceProvider serviceProvider,
        [Description("ID da task a cancelar.")] string taskId)
    {
        var taskService = serviceProvider.GetRequiredService<IBackgroundTaskService>();
        if (string.IsNullOrWhiteSpace(taskId))
            return JsonSerializer.Serialize(new { erro = "ID da task não informado." });

        var cancelled = await taskService.CancelTaskAsync(taskId.Trim());

        return cancelled
            ? JsonSerializer.Serialize(new { sucesso = true, mensagem = $"Task '{taskId}' cancelada." })
            : JsonSerializer.Serialize(new { sucesso = false, mensagem = $"Não foi possível cancelar a task '{taskId}'." });
    }
}
