using Gestcom.Domain.Entidades;

namespace Gestcom.Domain.Servicos;

public sealed class ResultadoBaixaEstoque
{
    public required string CodigoProduto { get; init; }
    public decimal EstoqueAnterior { get; init; }
    public decimal EstoqueNovo { get; init; }
    public decimal QuantidadeVendida { get; init; }
    public decimal QuantidadeBaixada { get; init; }
    public bool AplicouQuebra { get; init; }
    public SugestaoReposicao? Sugestao { get; init; }
}
