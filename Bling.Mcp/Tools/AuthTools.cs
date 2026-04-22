using System.ComponentModel;
using System.Text.Json;
using Bling.Application.Interfaces;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para autenticação OAuth do Bling.
/// Delega exclusivamente para a interface IAuthService da Application layer.
/// </summary>
[McpServerToolType]
public static class AuthTools
{
    [McpServerTool(Name = "verificar_autenticacao"), Description(
        "Verifica o status da autenticação OAuth com o Bling. " +
        "Se o token não estiver válido, tenta renovar automaticamente. " +
        "Retorna se está autenticado e quando o token expira.")]
    public static async Task<string> VerificarAutenticacao(
        IAuthService authService)
    {
        var tokenValido = authService.IsTokenValid();

        if (!tokenValido)
        {
            tokenValido = await authService.EnsureValidTokenAsync();
        }

        return JsonSerializer.Serialize(new
        {
            autenticado = tokenValido,
            tokenExpiraEm = authService.TokenExpiresAt.ToString("yyyy-MM-dd HH:mm:ss"),
            mensagem = tokenValido
                ? "Token OAuth válido e ativo."
                : "Falha na autenticação. Verifique BLING_CLIENT_ID e BLING_CLIENT_SECRET."
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
