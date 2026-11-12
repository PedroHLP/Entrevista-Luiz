using FluentAssertions;
using Gestcom.Domain.Regras;

namespace Gestcom.UnitTests;

public class RegrasFiscaisTests
{
    [Fact]
    public void Mesma_uf_usa_cfop_5102_e_icms_18()
    {
        RegrasFiscais.Cfop("SP").Should().Be("5102");
        RegrasFiscais.AliquotaIcms("10063021", "SP").Should().Be(18m);
    }

    [Fact]
    public void Outra_uf_usa_cfop_6102_e_icms_12()
    {
        RegrasFiscais.Cfop("PR").Should().Be("6102");
        RegrasFiscais.AliquotaIcms("10063021", "PR").Should().Be(12m);
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("PR")]
    [InlineData("MG")]
    public void Ncm_1905_tem_aliquota_7_em_qualquer_uf(string uf)
    {
        RegrasFiscais.AliquotaIcms("19059090", uf).Should().Be(7m);
        RegrasFiscais.Cfop(uf).Should().Be(uf == "SP" ? "5102" : "6102");
    }
}
