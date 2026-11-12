using FluentAssertions;
using Gestcom.Domain.Regras;

namespace Gestcom.UnitTests;

public class ArredondamentoLegadoTests
{
    [Fact]
    public void RoundVfp_meio_vai_para_longe_de_zero_nao_para_o_par()
    {
        // 1,125 com MidpointRounding.ToEven (padrão .NET) seria 1,12.
        // O ROUND do VFP e o do Gestcom vão para 1,13.
        ArredondamentoLegado.RoundVfp(1.125m).Should().Be(1.13m);
        Math.Round(1.125m, 2, MidpointRounding.ToEven).Should().Be(1.12m);
    }

    [Fact]
    public void Icms_da_agua_reproduz_o_caso_1_125()
    {
        ArredondamentoLegado.RoundIcms(6.25m * 18m / 100m).Should().Be(1.13m);
    }

    [Fact]
    public void Trunca2_nao_arredonda_para_cima()
    {
        // 47,94 * 5% = 2,397 → o comercial pediu para NÃO virar 2,40
        ArredondamentoLegado.Trunca2(2.397m).Should().Be(2.39m);
        Math.Round(2.397m, 2, MidpointRounding.AwayFromZero).Should().Be(2.40m);
    }

    [Fact]
    public void RoundGestcom_aplica_o_ajuste_de_ponto_flutuante()
    {
        ArredondamentoLegado.RoundGestcom(24.90m * 12m).Should().Be(298.80m);
        ArredondamentoLegado.AjustePontoFlutuante.Should().Be(0.00001m);
    }
}
