using Gestcom.Application.Abstracoes;
using Gestcom.Domain.Entidades;

namespace Gestcom.Application.Servicos;

public sealed class ReposicaoServico
{
    private readonly ISugestaoRepositorio _sugestoes;

    public ReposicaoServico(ISugestaoRepositorio sugestoes) => _sugestoes = sugestoes;

    public Task<IReadOnlyList<SugestaoReposicao>> ListarAsync(CancellationToken ct = default)
        => _sugestoes.ListarAsync(ct);
}
