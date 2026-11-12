namespace Gestcom.Domain.Entidades;

public class Pedido
{
    public int Numero { get; set; }
    public string CodigoCliente { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    /// <summary>A = Aberto, F = Fechado (baixado no estoque), como PEDIDOS.STATUS.</summary>
    public string Status { get; set; } = "A";
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorIcms { get; set; }
    public decimal ValorTotal { get; set; }
    public string Cfop { get; set; } = string.Empty;
    public List<ItemPedido> Itens { get; set; } = new();

    /// <summary>
    /// Parcela do desconto de cabeçalho aplicada por spc_desc_funcionario (não existe como campo na DBF).
    /// Mantido em memória para rastreabilidade. O valor já está somado em <see cref="ValorDesconto"/>.
    /// </summary>
    public decimal ValorDescontoFuncionario { get; set; }
}
