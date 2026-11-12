using Gestcom.Domain.Entidades;

namespace Gestcom.Domain.Servicos;

public sealed class ItemCalculoEntrada
{
    public required Produto Produto { get; init; }
    public required decimal Quantidade { get; init; }
}
