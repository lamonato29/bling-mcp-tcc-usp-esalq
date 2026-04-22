using System.Text.Json.Nodes;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Infrastructure.BlingApi.Repositories;

/// <summary>
/// Implementação do repositório de produtos usando a API Bling v3.
/// </summary>
public class BlingProdutoRepository : IProdutoRepository
{
    private readonly IBlingApiGateway _api;

    public BlingProdutoRepository(IBlingApiGateway api)
    {
        _api = api;
    }

    public async Task<List<Produto>> ListarAsync(ProdutoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue)
    {
        var queryParams = BuildQueryParams(filtros);
        var allProdutos = new List<Produto>();
        var pagina = 1;

        while (pagina <= maxPaginas)
        {
            queryParams["pagina"] = pagina.ToString();
            queryParams["limite"] = Math.Min(filtros.Limite, 100).ToString();

            var response = await _api.GetAsync("/produtos", queryParams);
            if (response == null) break;

            var dataArray = response["data"]?.AsArray();
            if (dataArray == null || dataArray.Count == 0) break;

            foreach (var item in dataArray)
            {
                if (item == null) continue;
                allProdutos.Add(MapProdutoResumido(item));
            }

            if (dataArray.Count < 100) break;
            pagina++;
            if (delayMs > 0) await Task.Delay(delayMs);
        }

        return allProdutos;
    }

    public async Task<Produto?> ObterPorIdAsync(long id)
    {
        var response = await _api.GetAsync($"/produtos/{id}");
        if (response == null) return null;

        var data = response["data"];
        if (data == null) return null;

        return MapProdutoDetalhado(data);
    }

    public async Task<Produto?> BuscarPorCodigoAsync(string codigo)
    {
        var response = await _api.GetAsync("/produtos", new Dictionary<string, string>
        {
            { "codigo", codigo },
            { "limite", "1" }
        });

        if (response == null) return null;
        var dataArray = response["data"]?.AsArray();
        if (dataArray == null || dataArray.Count == 0) return null;

        var id = dataArray[0]?["id"]?.GetValue<long>() ?? 0;
        if (id == 0) return null;

        return await ObterPorIdAsync(id);
    }

    public async Task<List<EstoqueItem>> ObterSaldoEstoqueAsync(long idProduto)
    {
        var response = await _api.GetAsync("/estoques/saldos", new Dictionary<string, string>
        {
            { "idsProdutos[]", idProduto.ToString() }
        });

        if (response == null) return new List<EstoqueItem>();

        var dataArray = response["data"]?.AsArray();
        if (dataArray == null) return new List<EstoqueItem>();

        var saldos = new List<EstoqueItem>();
        foreach (var item in dataArray)
        {
            if (item == null) continue;
            saldos.Add(new EstoqueItem
            {
                Produto = new EstoqueProduto
                {
                    Id = item["produto"]?["id"]?.GetValue<long>() ?? 0
                },
                Deposito = new Deposito
                {
                    Id = item["deposito"]?["id"]?.GetValue<long>() ?? 0,
                    Descricao = item["deposito"]?["descricao"]?.GetValue<string>()
                },
                SaldoFisicoTotal = item["saldoFisicoTotal"]?.GetValue<decimal>() ?? 0,
                SaldoVirtualTotal = item["saldoVirtualTotal"]?.GetValue<decimal>() ?? 0
            });
        }

        return saldos;
    }

    #region Query Params

    private static Dictionary<string, string> BuildQueryParams(ProdutoFiltrosDto filtros)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "pagina", filtros.Pagina.ToString() },
            { "limite", Math.Min(filtros.Limite, 100).ToString() }
        };

        if (filtros.Criterio.HasValue)
            queryParams["criterio"] = filtros.Criterio.Value.ToString();
        if (!string.IsNullOrEmpty(filtros.Tipo))
            queryParams["tipo"] = filtros.Tipo;
        if (filtros.IdCategoria.HasValue)
            queryParams["idCategoria"] = filtros.IdCategoria.Value.ToString();
        if (!string.IsNullOrEmpty(filtros.Nome))
            queryParams["nome"] = filtros.Nome;
        if (filtros.DataAlteracaoInicial.HasValue)
            queryParams["dataAlteracaoInicial"] = filtros.DataAlteracaoInicial.Value.ToString("yyyy-MM-dd");
        if (filtros.DataAlteracaoFinal.HasValue)
            queryParams["dataAlteracaoFinal"] = filtros.DataAlteracaoFinal.Value.ToString("yyyy-MM-dd");

        return queryParams;
    }

    #endregion

    #region JSON Mapping

    private static Produto MapProdutoResumido(JsonNode item)
    {
        return new Produto
        {
            Id = item["id"]?.GetValue<long>() ?? 0,
            Codigo = item["codigo"]?.GetValue<string>(),
            Nome = item["nome"]?.GetValue<string>(),
            Preco = item["preco"]?.GetValue<decimal>() ?? 0,
            PrecoCusto = item["precoCusto"]?.GetValue<decimal>() ?? 0,
            Unidade = item["unidade"]?.GetValue<string>(),
            Tipo = BlingApiMapper.TipoProdutoFromApi(item["tipo"]?.GetValue<string>()),
            Situacao = BlingApiMapper.SituacaoProdutoFromApi(item["situacao"]?.GetValue<string>()),
            Formato = BlingApiMapper.FormatoProdutoFromApi(item["formato"]?.GetValue<string>()),
            ImagemURL = item["imagemURL"]?.GetValue<string>(),
        };
    }

    private static Produto MapProdutoDetalhado(JsonNode data)
    {
        var produto = new Produto
        {
            Id = data["id"]?.GetValue<long>() ?? 0,
            Codigo = data["codigo"]?.GetValue<string>(),
            Nome = data["nome"]?.GetValue<string>(),
            Preco = data["preco"]?.GetValue<decimal>() ?? 0,
            PrecoCusto = data["precoCusto"]?.GetValue<decimal>() ?? 0,
            Unidade = data["unidade"]?.GetValue<string>(),
            Tipo = BlingApiMapper.TipoProdutoFromApi(data["tipo"]?.GetValue<string>()),
            Situacao = BlingApiMapper.SituacaoProdutoFromApi(data["situacao"]?.GetValue<string>()),
            Formato = BlingApiMapper.FormatoProdutoFromApi(data["formato"]?.GetValue<string>()),
            DescricaoCurta = data["descricaoCurta"]?.GetValue<string>(),
            Gtin = data["gtin"]?.GetValue<string>(),
            PesoLiquido = data["pesoLiquido"]?.GetValue<decimal>() ?? 0,
            PesoBruto = data["pesoBruto"]?.GetValue<decimal>() ?? 0,
            Marca = data["marca"]?.GetValue<string>(),
            ImagemURL = data["imagemURL"]?.GetValue<string>(),
            Observacoes = data["observacoes"]?.GetValue<string>(),
            FreteGratis = data["freteGratis"]?.GetValue<bool>() ?? false,
        };

        var estoque = data["estoque"];
        if (estoque != null)
        {
            produto.Estoque = new ProdutoEstoque
            {
                Minimo = estoque["minimo"]?.GetValue<decimal>() ?? 0,
                Maximo = estoque["maximo"]?.GetValue<decimal>() ?? 0,
                SaldoVirtualTotal = estoque["saldoVirtualTotal"]?.GetValue<decimal>() ?? 0,
                Localizacao = estoque["localizacao"]?.GetValue<string>(),
            };
        }

        var categoria = data["categoria"];
        if (categoria != null)
        {
            produto.Categoria = new ProdutoCategoria
            {
                Id = categoria["id"]?.GetValue<long>() ?? 0,
                Nome = categoria["nome"]?.GetValue<string>()
            };
        }

        return produto;
    }

    #endregion
}
