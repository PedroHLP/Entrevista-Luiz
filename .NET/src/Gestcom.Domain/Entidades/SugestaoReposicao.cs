namespace Gestcom.Domain.Entidades;

public class SugestaoReposicao
{
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal QuantidadeAtual { get; set; }
    public decimal QuantidadeMinima { get; set; }
    public decimal QuantidadeSugerida { get; set; }
    public DateTime Data { get; set; }
}
