using Gestcom.Domain.Entidades;

namespace Gestcom.Application.Abstracoes;

public interface ISugestaoRepositorio
{
    Task<SugestaoReposicao?> ObterPorProdutoAsync(string codigoProduto, CancellationToken ct = default);
    Task<IReadOnlyList<SugestaoReposicao>> ListarAsync(CancellationToken ct = default);
    Task SalvarAsync(SugestaoReposicao sugestao, CancellationToken ct = default);
}
