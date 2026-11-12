namespace Gestcom.Application.Abstracoes;

public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken ct = default);
}
