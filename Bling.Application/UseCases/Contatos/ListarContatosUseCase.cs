using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Contatos;

/// <summary>
/// Use Case: Listar contatos com filtros
/// </summary>
public class ListarContatosUseCase
{
    private readonly IContatoRepository _contatoRepository;

    public ListarContatosUseCase(IContatoRepository contatoRepository)
    {
        _contatoRepository = contatoRepository;
    }

    public async Task<List<Contato>> ExecuteAsync(int pagina = 1, int limite = 100, string? pesquisa = null)
    {
        return await _contatoRepository.ListarAsync(pagina, limite, pesquisa);
    }
}
