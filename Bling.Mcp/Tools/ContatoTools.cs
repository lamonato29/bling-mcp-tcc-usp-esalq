using System.ComponentModel;
using System.Text.Json;
using Bling.Application.UseCases.Contatos;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para o domínio de Contatos.
/// Delega exclusivamente para os Use Cases da Application layer.
/// </summary>
[McpServerToolType]
public static class ContatoTools
{
    [McpServerTool(Name = "listar_contatos"), Description(
        "Lista contatos (clientes/fornecedores) cadastrados no Bling. " +
        "Retorna uma lista com id, nome, código, situação, documento, telefone e email.")]
    public static async Task<string> ListarContatos(
        ListarContatosUseCase useCase,
        [Description("Termo de pesquisa (nome, documento, etc.). Opcional.")] string? pesquisa = null,
        [Description("Número da página. Padrão: 1.")] int pagina = 1,
        [Description("Limite de resultados por página (máx 100). Padrão: 100.")] int limite = 100)
    {
        var contatos = await useCase.ExecuteAsync(pagina, Math.Min(limite, 100), pesquisa);

        return JsonSerializer.Serialize(new
        {
            total = contatos.Count,
            contatos = contatos.Select(c => new
            {
                c.Id,
                c.Nome,
                c.Codigo,
                c.Situacao,
                c.NumeroDocumento,
                c.Telefone,
                c.Email
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
