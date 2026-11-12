using FluentAssertions;
using Gestcom.Domain.Regras;
using Gestcom.Domain.Servicos;

namespace Gestcom.UnitTests;

public class ProcessadorBaixaEstoqueTests
{
    [Fact]
    public void Refrigerante_baixa_quantidade_mais_uma_unidade_de_quebra()
    {
        var produto = CatalogoHomologacao.Produto("0004"); // estoque 18, min 24, NCM 2202
        var resultado = ProcessadorBaixaEstoque.Aplicar(produto, 5m, new DateTime(2026, 11, 12));

        resultado.AplicouQuebra.Should().BeTrue();
        resultado.QuantidadeBaixada.Should().Be(6m);
        resultado.EstoqueNovo.Should().Be(12m);
        produto.Estoque.Should().Be(12m);
        resultado.Sugestao.Should().NotBeNull();
        resultado.Sugestao!.QuantidadeSugerida.Should().Be(36m); // 24*2 - 12
    }

    [Fact]
    public void Venda_do_estoque_inteiro_de_refrigerante_deixa_saldo_negativo()
    {
        var produto = CatalogoHomologacao.Produto("0004");
        var resultado = ProcessadorBaixaEstoque.Aplicar(produto, 18m, DateTime.Today);

        resultado.EstoqueNovo.Should().Be(-1m);
        resultado.Sugestao!.QuantidadeSugerida.Should().Be(49m); // 48 - (-1)
    }

    [Fact]
    public void Validacao_aceitaria_os_18_mesmo_com_quebra_posterior()
    {
        var produto = CatalogoHomologacao.Produto("0004");
        (18m > produto.Estoque).Should().BeFalse();
        (19m > produto.Estoque).Should().BeTrue();
    }

    [Fact]
    public void Acucar_com_minimo_zero_nunca_gera_sugestao()
    {
        var produto = CatalogoHomologacao.Produto("0007");
        produto.Estoque = 0m;
        var resultado = ProcessadorBaixaEstoque.Aplicar(produto, 1m, DateTime.Today);
        resultado.Sugestao.Should().BeNull();
        RegrasEstoque.QuantidadeSugerida(0m, 0m).Should().BeNull();
    }

    [Fact]
    public void Arroz_acima_do_minimo_nao_gera_sugestao()
    {
        var produto = CatalogoHomologacao.Produto("0001");
        var resultado = ProcessadorBaixaEstoque.Aplicar(produto, 12m, DateTime.Today);
        resultado.AplicouQuebra.Should().BeFalse();
        resultado.EstoqueNovo.Should().Be(38m);
        resultado.Sugestao.Should().BeNull();
    }

    [Fact]
    public void Cafe_ja_abaixo_do_minimo_sugere_reposicao()
    {
        var produto = CatalogoHomologacao.Produto("0005"); // 8, min 10
        var resultado = ProcessadorBaixaEstoque.Aplicar(produto, 2m, DateTime.Today);
        resultado.EstoqueNovo.Should().Be(6m);
        resultado.Sugestao!.QuantidadeSugerida.Should().Be(14m);
    }
}
