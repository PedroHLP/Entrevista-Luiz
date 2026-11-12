using Gestcom.Domain.Entidades;
using Gestcom.Domain.Servicos;

namespace Gestcom.BddTests.StepDefinitions;

public sealed class ContextoPedido
{
    public Cliente? Cliente { get; set; }
    public Dictionary<string, Produto> Produtos { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Pedido? Pedido { get; set; }
    public ResultadoBaixaEstoque? Baixa { get; set; }
    public Exception? Erro { get; set; }

    public Produto Produto(string codigo) => Produtos[codigo.Trim()];
}
