using Gestcom.Domain.Entidades;

namespace Gestcom.Application.Abstracoes;

public interface IPedidoRepositorio
{
    Task<int> ProximoNumeroAsync(CancellationToken ct = default);
    Task<Pedido?> ObterPorNumeroAsync(int numero, CancellationToken ct = default);
    Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Pedido pedido, CancellationToken ct = default);
}
