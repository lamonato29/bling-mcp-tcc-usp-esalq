using Bling.Application.DTOs;
using Bling.Application.Interfaces;
using Bling.Domain.Entities;

namespace Bling.Application.UseCases.Pedidos;

/// <summary>
/// Use Case: Listar pedidos de venda com filtros e paginação automática
/// </summary>
public class ListarPedidosUseCase
{
    private readonly IPedidoRepository _pedidoRepository;

    public ListarPedidosUseCase(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<List<Pedido>> ExecuteAsync(PedidoFiltrosDto filtros, int delayMs = 400, int maxPaginas = int.MaxValue, Action<int>? onPage = null)
    {
        return await _pedidoRepository.ListarAsync(filtros, delayMs, maxPaginas, onPage);
    }
}
