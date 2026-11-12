using FluentAssertions;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Regras;

namespace Gestcom.UnitTests;

public class RegrasDescontoTests
{
    [Theory]
    [InlineData(TipoCliente.Varejista, 12, 10)]
    [InlineData(TipoCliente.Varejista, 11, 5)]
    [InlineData(TipoCliente.Varejista, 6, 5)]
    [InlineData(TipoCliente.Varejista, 5, 0)]
    [InlineData(TipoCliente.Consumidor, 12, 0)]
    [InlineData(TipoCliente.Funcionario, 12, 0)]
    public void Faixa_de_varejista_nao_e_cumulativa(TipoCliente tipo, int qtd, int percentualEsperado)
    {
        RegrasDesconto.PercentualFaixa(tipo, qtd).Should().Be(percentualEsperado);
    }

    [Fact]
    public void Funcionario_recebe_5_por_cento_quando_subtotal_atinge_50()
    {
        RegrasDesconto.DescontoFuncionario(TipoCliente.Funcionario, 67.60m, 0m).Should().Be(3.38m);
    }

    [Fact]
    public void Funcionario_nao_recebe_desconto_abaixo_do_piso()
    {
        RegrasDesconto.DescontoFuncionario(TipoCliente.Funcionario, 47.94m, 0m).Should().Be(0m);
    }

    [Fact]
    public void Consumidor_nunca_entra_no_desconto_de_funcionario()
    {
        RegrasDesconto.DescontoFuncionario(TipoCliente.Consumidor, 200m, 0m).Should().Be(0m);
    }
}
