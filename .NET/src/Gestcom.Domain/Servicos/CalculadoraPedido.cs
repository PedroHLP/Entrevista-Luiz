using Gestcom.Domain.Entidades;
using Gestcom.Domain.Excecoes;
using Gestcom.Domain.Regras;

namespace Gestcom.Domain.Servicos;

/// <summary>
/// Reproduz o fluxo de ped_inclui.prg + spc_calc_icms.prg + spc_desc_funcionario.prg,
/// sem persistir nem baixar estoque (isso fica no ProcessadorBaixaEstoque / PedidoServico).
/// </summary>
public sealed class CalculadoraPedido
{
    public Pedido Calcular(
        Cliente cliente,
        IReadOnlyList<ItemCalculoEntrada> itens,
        DateTime? data = null,
        int numero = 0)
    {
        if (itens is null || itens.Count == 0)
            throw new PedidoInvalidoException("Pedido cancelado (nenhum item).");

        var pedido = new Pedido
        {
            Numero = numero,
            CodigoCliente = cliente.Codigo,
            Data = (data ?? DateTime.Today).Date,
            Status = "A",
            Cfop = RegrasFiscais.Cfop(cliente.Uf)
        };

        var nItem = 0;
        decimal totBruto = 0m, totDesc = 0m, totIcms = 0m;

        foreach (var entrada in itens)
        {
            if (entrada.Quantidade <= 0)
                throw new PedidoInvalidoException("Quantidade inválida.");
            if (!entrada.Produto.Ativo)
                throw new PedidoInvalidoException("Produto inexistente ou inativo.");

            nItem++;
            var perc = RegrasDesconto.PercentualFaixa(cliente.Tipo, entrada.Quantidade);
            var vlrBruto = ArredondamentoLegado.RoundGestcom(entrada.Produto.Preco * entrada.Quantidade);
            var vlrDesc = RegrasDesconto.ValorDescontoFaixa(vlrBruto, perc);
            var vlrItem = ArredondamentoLegado.RoundGestcom(vlrBruto - vlrDesc);
            var aliq = RegrasFiscais.AliquotaIcms(entrada.Produto.Ncm, cliente.Uf);
            var vlrIcms = ArredondamentoLegado.RoundIcms(vlrItem * aliq / 100m);

            pedido.Itens.Add(new ItemPedido
            {
                NumeroPedido = numero,
                Item = nItem,
                CodigoProduto = entrada.Produto.Codigo,
                Quantidade = entrada.Quantidade,
                Preco = entrada.Produto.Preco,
                PercentualDesconto = perc,
                ValorDesconto = vlrDesc,
                ValorItem = vlrItem,
                AliquotaIcms = aliq,
                ValorIcms = vlrIcms
            });

            totBruto += vlrBruto;
            totDesc += vlrDesc;
            totIcms += vlrIcms;
        }

        totBruto = ArredondamentoLegado.RoundGestcom(totBruto);
        totDesc = ArredondamentoLegado.RoundGestcom(totDesc);
        totIcms = ArredondamentoLegado.RoundGestcom(totIcms);

        var descFuncionario = RegrasDesconto.DescontoFuncionario(cliente.Tipo, totBruto, totDesc);
        totDesc = ArredondamentoLegado.RoundGestcom(totDesc + descFuncionario);
        var totGeral = ArredondamentoLegado.RoundGestcom(totBruto - totDesc);

        pedido.ValorBruto = totBruto;
        pedido.ValorDesconto = totDesc;
        pedido.ValorIcms = totIcms;
        pedido.ValorTotal = totGeral;
        pedido.ValorDescontoFuncionario = descFuncionario;
        return pedido;
    }
}
