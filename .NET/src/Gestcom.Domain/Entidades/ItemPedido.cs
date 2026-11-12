namespace Gestcom.Domain.Entidades;

public class ItemPedido
{
    public int NumeroPedido { get; set; }
    public int Item { get; set; }
    public string CodigoProduto { get; set; } = string.Empty;
    public decimal Quantidade { get; set; }
    public decimal Preco { get; set; }
    public decimal PercentualDesconto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorItem { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal ValorIcms { get; set; }
}
