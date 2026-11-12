namespace Gestcom.Domain.Excecoes;

public class EstoqueInsuficienteException : Exception
{
    public string CodigoProduto { get; }
    public decimal QuantidadeSolicitada { get; }
    public decimal EstoqueDisponivel { get; }

    public EstoqueInsuficienteException(string codigoProduto, decimal quantidadeSolicitada, decimal estoqueDisponivel)
        : base($"Estoque insuficiente para o produto {codigoProduto.Trim()}. Disponível: {estoqueDisponivel:0.00}.")
    {
        CodigoProduto = codigoProduto;
        QuantidadeSolicitada = quantidadeSolicitada;
        EstoqueDisponivel = estoqueDisponivel;
    }
}
