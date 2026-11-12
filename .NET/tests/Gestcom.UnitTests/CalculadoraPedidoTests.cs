using FluentAssertions;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Excecoes;
using Gestcom.Domain.Servicos;

namespace Gestcom.UnitTests;

public class CalculadoraPedidoTests
{
    private readonly CalculadoraPedido _calculadora = new();

    [Fact]
    public void Varejista_sp_12_arroz_aplica_10_por_cento_e_icms_18()
    {
        var pedido = Calcular("0001", "0001", 12);

        pedido.Cfop.Should().Be("5102");
        pedido.ValorBruto.Should().Be(298.80m);
        pedido.ValorDesconto.Should().Be(29.88m);
        pedido.ValorTotal.Should().Be(268.92m);
        pedido.ValorIcms.Should().Be(48.41m);
        pedido.Itens.Single().PercentualDesconto.Should().Be(10m);
    }

    [Fact]
    public void Varejista_pr_6_paes_trunca_desconto_e_usa_icms_reduzido()
    {
        var pedido = Calcular("0002", "0003", 6);

        pedido.Cfop.Should().Be("6102");
        pedido.ValorBruto.Should().Be(47.94m);
        pedido.ValorDesconto.Should().Be(2.39m); // 2,397 truncado, não 2,40
        pedido.ValorTotal.Should().Be(45.55m);
        pedido.Itens.Single().AliquotaIcms.Should().Be(7m);
        pedido.ValorIcms.Should().Be(3.19m);
    }

    [Fact]
    public void Funcionario_4_cafes_aplica_5_por_cento_sem_recalcular_icms()
    {
        var pedido = Calcular("0004", "0005", 4);

        pedido.ValorBruto.Should().Be(67.60m);
        pedido.ValorDescontoFuncionario.Should().Be(3.38m);
        pedido.ValorDesconto.Should().Be(3.38m);
        pedido.ValorTotal.Should().Be(64.22m);
        // ICMS incide sobre 67,60 (antes do desconto de funcionário), não sobre 64,22
        pedido.ValorIcms.Should().Be(12.17m);
        pedido.Itens.Single().ValorIcms.Should().Be(12.17m);
    }

    [Fact]
    public void Funcionario_abaixo_do_piso_nao_recebe_desconto()
    {
        var pedido = Calcular("0004", "0003", 6);
        pedido.ValorBruto.Should().Be(47.94m);
        pedido.ValorDesconto.Should().Be(0m);
        pedido.ValorDescontoFuncionario.Should().Be(0m);
        pedido.Itens.Single().AliquotaIcms.Should().Be(7m);
        pedido.ValorIcms.Should().Be(3.36m);
    }

    [Fact]
    public void Consumidor_agua_arredonda_icms_1_125_para_1_13()
    {
        var pedido = Calcular("0003", "0006", 1);
        pedido.ValorTotal.Should().Be(6.25m);
        pedido.ValorIcms.Should().Be(1.13m);
        pedido.Itens.Single().AliquotaIcms.Should().Be(18m);
    }

    [Fact]
    public void Pedido_com_dois_itens_soma_totais_e_mantem_aliquotas_por_item()
    {
        var pedido = _calculadora.Calcular(
            CatalogoHomologacao.Cliente("0001"),
            new[]
            {
                new ItemCalculoEntrada { Produto = CatalogoHomologacao.Produto("0001"), Quantidade = 12 },
                new ItemCalculoEntrada { Produto = CatalogoHomologacao.Produto("0003"), Quantidade = 6 },
            });

        pedido.ValorBruto.Should().Be(346.74m);
        pedido.ValorDesconto.Should().Be(32.27m);
        pedido.ValorIcms.Should().Be(51.60m);
        pedido.ValorTotal.Should().Be(314.47m);
        pedido.Cfop.Should().Be("5102");
        pedido.Itens.Should().HaveCount(2);
        pedido.Itens[0].AliquotaIcms.Should().Be(18m);
        pedido.Itens[1].AliquotaIcms.Should().Be(7m);
    }

    [Fact]
    public void Varejista_abaixo_da_faixa_nao_tem_desconto()
    {
        var pedido = Calcular("0001", "0001", 5);
        pedido.ValorBruto.Should().Be(124.50m);
        pedido.ValorDesconto.Should().Be(0m);
        pedido.ValorIcms.Should().Be(22.41m);
    }

    [Fact]
    public void Pedido_sem_itens_e_rejeitado()
    {
        var act = () => _calculadora.Calcular(CatalogoHomologacao.Cliente("0001"), Array.Empty<ItemCalculoEntrada>());
        act.Should().Throw<PedidoInvalidoException>();
    }

    private Pedido Calcular(string cliente, string produto, decimal qtd)
        => _calculadora.Calcular(
            CatalogoHomologacao.Cliente(cliente),
            new[] { new ItemCalculoEntrada { Produto = CatalogoHomologacao.Produto(produto), Quantidade = qtd } });
}
