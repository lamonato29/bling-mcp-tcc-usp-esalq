using System.ComponentModel;
using System.Text.Json;
using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Application.UseCases.Produtos;
using ModelContextProtocol.Server;

namespace Bling.Mcp.Tools;

/// <summary>
/// MCP Tools para o domínio de Produtos.
/// Delega exclusivamente para os Use Cases e Repositories da Application layer.
/// </summary>
[McpServerToolType]
public static class ProdutoTools
{
    [McpServerTool(Name = "listar_produtos"), Description(
        "Lista produtos cadastrados no Bling com filtros opcionais. " +
        "Retorna uma lista de produtos com id, código, nome, preço, tipo e situação.")]
    public static async Task<string> ListarProdutos(
        ListarProdutosUseCase useCase,
        [Description("Nome do produto para busca. Opcional.")] string? nome = null,
        [Description("Tipo do produto (S=Serviço, P=Produto, N=Sem cadastro). Opcional.")] string? tipo = null,
        [Description("ID da categoria. Opcional.")] long? idCategoria = null,
        [Description("Data de alteração inicial (formato yyyy-MM-dd). Opcional.")] string? dataAlteracaoInicial = null,
        [Description("Data de alteração final (formato yyyy-MM-dd). Opcional.")] string? dataAlteracaoFinal = null,
        [Description("Limite de resultados por página (máx 100). Padrão: 100.")] int limite = 100,
        [Description("Máximo de páginas a buscar. Padrão: 5.")] int maxPaginas = 5)
    {
        var filtros = new ProdutoFiltrosDto
        {
            Limite = Math.Min(limite, 100),
            Nome = nome,
            Tipo = tipo,
            IdCategoria = idCategoria
        };

        if (DateTime.TryParse(dataAlteracaoInicial, out var dai)) filtros.DataAlteracaoInicial = dai;
        if (DateTime.TryParse(dataAlteracaoFinal, out var daf)) filtros.DataAlteracaoFinal = daf;

        var produtos = await useCase.ExecuteAsync(filtros, maxPaginas: maxPaginas);

        return JsonSerializer.Serialize(new
        {
            total = produtos.Count,
            produtos = produtos.Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nome,
                p.Preco,
                p.PrecoCusto,
                p.Unidade,
                Tipo = p.Tipo.ToString(),
                Situacao = p.Situacao.ToString(),
                Formato = p.Formato.ToString(),
                p.ImagemURL
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "obter_produto"), Description(
        "Obtém detalhes completos de um produto por ID ou código. " +
        "Retorna informações detalhadas incluindo estoque, categoria, dimensões e mais.")]
    public static async Task<string> ObterProduto(
        ObterProdutoUseCase useCase,
        [Description("ID do produto no Bling. Use este OU codigo.")] long? id = null,
        [Description("Código do produto (SKU). Use este OU id.")] string? codigo = null)
    {
        if (id == null && string.IsNullOrEmpty(codigo))
            return JsonSerializer.Serialize(new { erro = "Informe 'id' ou 'codigo' do produto." });

        var produto = id.HasValue
            ? await useCase.ExecuteByIdAsync(id.Value)
            : await useCase.ExecuteByCodigoAsync(codigo!);

        if (produto == null)
            return JsonSerializer.Serialize(new { erro = "Produto não encontrado." });

        return JsonSerializer.Serialize(new
        {
            produto.Id,
            produto.Codigo,
            produto.Nome,
            produto.Preco,
            produto.PrecoCusto,
            produto.Unidade,
            Tipo = produto.Tipo.ToString(),
            Situacao = produto.Situacao.ToString(),
            Formato = produto.Formato.ToString(),
            produto.DescricaoCurta,
            produto.Gtin,
            produto.PesoLiquido,
            produto.PesoBruto,
            produto.Marca,
            produto.ImagemURL,
            produto.Observacoes,
            produto.FreteGratis,
            Estoque = produto.Estoque != null ? new
            {
                produto.Estoque.Minimo,
                produto.Estoque.Maximo,
                produto.Estoque.SaldoVirtualTotal,
                produto.Estoque.Localizacao
            } : null,
            Categoria = produto.Categoria != null ? new
            {
                produto.Categoria.Id,
                produto.Categoria.Nome
            } : null
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool(Name = "obter_saldo_estoque"), Description(
        "Obtém o saldo de estoque de um produto por ID. " +
        "Retorna quantidade em estoque por depósito.")]
    public static async Task<string> ObterSaldoEstoque(
        IProdutoRepository produtoRepository,
        [Description("ID do produto no Bling.")] long idProduto)
    {
        var saldos = await produtoRepository.ObterSaldoEstoqueAsync(idProduto);

        if (saldos.Count == 0)
            return JsonSerializer.Serialize(new { idProduto, saldos = Array.Empty<object>() });

        return JsonSerializer.Serialize(new
        {
            idProduto,
            total = saldos.Count,
            saldos = saldos.Select(s => new
            {
                IdProduto = s.Produto?.Id ?? 0,
                IdDeposito = s.Deposito?.Id ?? 0,
                NomeDeposito = s.Deposito?.Descricao,
                SaldoFisico = s.SaldoFisicoTotal,
                SaldoVirtual = s.SaldoVirtualTotal
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
