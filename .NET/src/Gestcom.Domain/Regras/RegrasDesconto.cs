using Gestcom.Domain.Entidades;

namespace Gestcom.Domain.Regras;

/// <summary>
/// Descontos de faixa (funcoes.prg / PercDescontoFaixa) e de funcionário (spc_desc_funcionario.prg).
/// </summary>
public static class RegrasDesconto
{
    public const decimal PercentualFaixa6 = 5m;
    public const decimal PercentualFaixa12 = 10m;
    public const decimal QuantidadeFaixa6 = 6m;
    public const decimal QuantidadeFaixa12 = 12m;
    public const decimal PercentualFuncionario = 5m;
    public const decimal PisoDescontoFuncionario = 50m;

    /// <summary>
    /// Percentual de desconto do ITEM. Apenas varejista. Faixas não cumulativas.
    /// </summary>
    public static decimal PercentualFaixa(TipoCliente tipo, decimal quantidade)
    {
        if (tipo != TipoCliente.Varejista)
            return 0m;
        if (quantidade >= QuantidadeFaixa12)
            return PercentualFaixa12;
        if (quantidade >= QuantidadeFaixa6)
            return PercentualFaixa6;
        return 0m;
    }

    public static decimal ValorDescontoFaixa(decimal valorBruto, decimal percentual)
        => ArredondamentoLegado.Trunca2(valorBruto * percentual / 100m);

    /// <summary>
    /// Desconto adicional de cabeçalho para funcionário, se subtotal &gt;= 50.
    /// Usa ROUND puro (sem +0.00001), como spc_desc_funcionario.prg.
    /// Não recalcula ICMS — isso é particularidade intencional do legado.
    /// </summary>
    public static decimal DescontoFuncionario(TipoCliente tipo, decimal valorBruto, decimal descontoJaAplicado)
    {
        if (tipo != TipoCliente.Funcionario)
            return 0m;

        var subtotal = ArredondamentoLegado.RoundVfp(valorBruto - descontoJaAplicado);
        if (subtotal < PisoDescontoFuncionario)
            return 0m;

        return ArredondamentoLegado.RoundVfp(subtotal * PercentualFuncionario / 100m);
    }
}
