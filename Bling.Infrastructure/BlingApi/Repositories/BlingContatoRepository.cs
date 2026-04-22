using System.Text.Json.Nodes;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Infrastructure.BlingApi.Repositories;

/// <summary>
/// Implementação do repositório de contatos usando a API Bling v3.
/// </summary>
public class BlingContatoRepository : IContatoRepository
{
    private readonly IBlingApiGateway _api;

    public BlingContatoRepository(IBlingApiGateway api)
    {
        _api = api;
    }

    public async Task<List<Contato>> ListarAsync(int pagina = 1, int limite = 100, string? pesquisa = null)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pagina", pagina.ToString() },
            { "limite", Math.Min(limite, 100).ToString() }
        };

        if (!string.IsNullOrEmpty(pesquisa))
            queryParams["pesquisa"] = pesquisa;

        var response = await _api.GetAsync("/contatos", queryParams);
        if (response == null) return new List<Contato>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<Contato>();

        var contatos = new List<Contato>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            contatos.Add(new Contato
            {
                Id = item["id"]?.GetValue<long>() ?? 0,
                Nome = item["nome"]?.GetValue<string>(),
                Codigo = item["codigo"]?.GetValue<string>(),
                Situacao = item["situacao"]?.GetValue<string>(),
                NumeroDocumento = item["numeroDocumento"]?.GetValue<string>(),
                Telefone = item["telefone"]?.GetValue<string>(),
                Email = item["email"]?.GetValue<string>(),
            });
        }

        return contatos;
    }

    public async Task<Contato?> BuscarPorDocumentoAsync(string documento)
    {
        var contatos = await ListarAsync(pesquisa: documento);
        return contatos.FirstOrDefault();
    }
}
