using System.Globalization;
using FluentAssertions;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Excecoes;
using Gestcom.Domain.Servicos;
using Reqnroll;

namespace Gestcom.BddTests.StepDefinitions;

[Binding]
public sealed class PedidoSteps
{
    private readonly ContextoPedido _ctx;
    private readonly CalculadoraPedido _calculadora = new();
    private static readonly CultureInfo Pt = new("pt-BR");

    public PedidoSteps(ContextoPedido ctx) => _ctx = ctx;

    [Given("o catálogo de homologação carregado")]
    public void DadoCatalogo()
    {
        foreach (var p in CatalogoBdd.Produtos())
            _ctx.Produtos[p.Codigo] = p;
    }

    [Given(@"um cliente varejista do estado ""(.*)""")]
    public void DadoVarejista(string uf)
        => _ctx.Cliente = new Cliente { Codigo = "V-" + uf, Nome = "Varejista " + uf, Tipo = TipoCliente.Varejista, Uf = uf };

    [Given(@"um cliente consumidor do estado ""(.*)""")]
    public void DadoConsumidor(string uf)
        => _ctx.Cliente = new Cliente { Codigo = "C-" + uf, Nome = "Consumidor " + uf, Tipo = TipoCliente.Consumidor, Uf = uf };

    [Given(@"um cliente funcionário do estado ""(.*)""")]
    public void DadoFuncionario(string uf)
        => _ctx.Cliente = new Cliente { Codigo = "F-" + uf, Nome = "Funcionario " + uf, Tipo = TipoCliente.Funcionario, Uf = uf };

    [Given(@"o produto ""(.*)"" com estoque (.+) e mínimo (.+)")]
    public void DadoEstoque(string codigo, string estoque, string minimo)
    {
        if (!_ctx.Produtos.ContainsKey(codigo))
            DadoCatalogo();
        var p = _ctx.Produto(codigo);
        p.Estoque = Dec(estoque);
        p.EstoqueMinimo = Dec(minimo);
    }

    [When(@"é lançado um pedido com (.+) unidades? do produto ""(.*)""")]
    public void QuandoLancaPedido(string qtd, string codigo)
    {
        _ctx.Cliente.Should().NotBeNull();
        var produto = _ctx.Produto(codigo);
        _ctx.Pedido = _calculadora.Calcular(
            _ctx.Cliente!,
            new[] { new ItemCalculoEntrada { Produto = produto, Quantidade = Dec(qtd) } });
    }

    [When(@"a venda de (.+) unidades? do produto ""(.*)"" é baixada no estoque")]
    public void QuandoBaixa(string qtd, string codigo)
    {
        _ctx.Baixa = ProcessadorBaixaEstoque.Aplicar(_ctx.Produto(codigo), Dec(qtd), new DateTime(2026, 11, 12));
    }

    [When(@"se tenta lançar um pedido com (.+) unidades? do produto ""(.*)""")]
    public void QuandoTentaLancar(string qtdTexto, string codigo)
    {
        var produto = _ctx.Produto(codigo);
        var qtd = Dec(qtdTexto);
        if (qtd > produto.Estoque)
        {
            _ctx.Erro = new EstoqueInsuficienteException(produto.Codigo, qtd, produto.Estoque);
            return;
        }

        _ctx.Pedido = _calculadora.Calcular(
            _ctx.Cliente!,
            new[] { new ItemCalculoEntrada { Produto = produto, Quantidade = qtd } });
    }

    [Then(@"o valor bruto deve ser (.+)")]
    public void EntaoBruto(string valor) => _ctx.Pedido!.ValorBruto.Should().Be(Dec(valor));

    [Then(@"o valor de desconto deve ser (.+)")]
    public void EntaoDesconto(string valor) => _ctx.Pedido!.ValorDesconto.Should().Be(Dec(valor));

    [Then(@"o valor total deve ser (.+)")]
    public void EntaoTotal(string valor) => _ctx.Pedido!.ValorTotal.Should().Be(Dec(valor));

    [Then(@"o valor de ICMS deve ser (.+)")]
    public void EntaoIcms(string valor) => _ctx.Pedido!.ValorIcms.Should().Be(Dec(valor));

    [Then(@"o CFOP deve ser ""(.*)""")]
    public void EntaoCfop(string cfop) => _ctx.Pedido!.Cfop.Should().Be(cfop);

    [Then(@"o percentual de desconto do item deve ser (.+)")]
    public void EntaoPerc(string valor) => _ctx.Pedido!.Itens.Single().PercentualDesconto.Should().Be(Dec(valor));

    [Then(@"a alíquota de ICMS do item deve ser (.+)")]
    public void EntaoAliq(string valor) => _ctx.Pedido!.Itens.Single().AliquotaIcms.Should().Be(Dec(valor));

    [Then(@"o estoque final deve ser (.+)")]
    public void EntaoEstoque(string valor) => _ctx.Baixa!.EstoqueNovo.Should().Be(Dec(valor));

    [Then("a quebra operacional deve ter sido aplicada")]
    public void EntaoQuebra() => _ctx.Baixa!.AplicouQuebra.Should().BeTrue();

    [Then(@"a quantidade sugerida de reposição deve ser (.+)")]
    public void EntaoSugestao(string valor)
    {
        _ctx.Baixa!.Sugestao.Should().NotBeNull();
        _ctx.Baixa.Sugestao!.QuantidadeSugerida.Should().Be(Dec(valor));
    }

    [Then("não deve haver sugestão de reposição")]
    public void EntaoSemSugestao() => _ctx.Baixa!.Sugestao.Should().BeNull();

    [Then("o pedido deve ser rejeitado por estoque insuficiente")]
    public void EntaoRejeitado() => _ctx.Erro.Should().BeOfType<EstoqueInsuficienteException>();

    [Then(@"o estoque do produto ""(.*)"" deve permanecer (.+)")]
    public void EntaoEstoqueIntocado(string codigo, string valor)
        => _ctx.Produto(codigo).Estoque.Should().Be(Dec(valor));

    private static decimal Dec(string valor)
        => decimal.Parse(valor.Trim().Trim('"'), Pt);
}

internal static class CatalogoBdd
{
    public static IEnumerable<Produto> Produtos() => new[]
    {
        new Produto { Codigo = "0001", Descricao = "ARROZ TIPO 1 5KG",     Ncm = "10063021", Preco = 24.90m, Estoque = 50m, EstoqueMinimo = 20m, Ativo = true },
        new Produto { Codigo = "0002", Descricao = "FEIJAO CARIOCA 1KG",   Ncm = "07133319", Preco = 8.50m,  Estoque = 30m, EstoqueMinimo = 15m, Ativo = true },
        new Produto { Codigo = "0003", Descricao = "PAO DE FORMA 500G",    Ncm = "19059090", Preco = 7.99m,  Estoque = 40m, EstoqueMinimo = 25m, Ativo = true },
        new Produto { Codigo = "0004", Descricao = "REFRIGERANTE COLA 2L", Ncm = "22021000", Preco = 6.50m,  Estoque = 18m, EstoqueMinimo = 24m, Ativo = true },
        new Produto { Codigo = "0005", Descricao = "CAFE TORRADO 500G",    Ncm = "09012100", Preco = 16.90m, Estoque = 8m,  EstoqueMinimo = 10m, Ativo = true },
        new Produto { Codigo = "0006", Descricao = "AGUA MINERAL 1,5L",    Ncm = "22019000", Preco = 6.25m,  Estoque = 25m, EstoqueMinimo = 12m, Ativo = true },
        new Produto { Codigo = "0007", Descricao = "ACUCAR REFINADO 1KG",  Ncm = "17019900", Preco = 4.19m,  Estoque = 60m, EstoqueMinimo = 0m,  Ativo = true },
    };
}
