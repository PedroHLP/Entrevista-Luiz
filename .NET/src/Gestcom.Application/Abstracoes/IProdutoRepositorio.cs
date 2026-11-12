using Gestcom.Domain.Entidades;

namespace Gestcom.Application.Abstracoes;

public interface IProdutoRepositorio
{
    Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Produto produto, CancellationToken ct = default);
    Task AtualizarAsync(Produto produto, CancellationToken ct = default);
}
