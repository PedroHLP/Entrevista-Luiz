using Gestcom.Domain.Entidades;

namespace Gestcom.Application.Abstracoes;

public interface IClienteRepositorio
{
    Task<Cliente?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);
}
