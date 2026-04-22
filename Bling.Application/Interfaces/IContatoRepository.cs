using Bling.Domain.Entities;

namespace Bling.Application.Interfaces;

/// <summary>
/// Port para acesso a dados de contatos (clientes/fornecedores).
/// </summary>
public interface IContatoRepository
{
    /// <summary>
    /// Lista contatos com filtros
    /// </summary>
    Task<List<Contato>> ListarAsync(int pagina = 1, int limite = 100, string? pesquisa = null);

    /// <summary>
    /// Busca contato por documento (CPF/CNPJ)
    /// </summary>
    Task<Contato?> BuscarPorDocumentoAsync(string documento);
}
