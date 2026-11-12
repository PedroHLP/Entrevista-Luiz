namespace Gestcom.Domain.Regras;

/// <summary>
/// Reproduz o arredondamento do Gestcom (Visual FoxPro 9).
/// Fonte: funcoes.prg (RoundGestcom, Trunca2) e spc_calc_icms.prg (ROUND puro).
/// </summary>
public static class ArredondamentoLegado
{
    /// <summary>
    /// Compensação de ponto flutuante usada no legado desde 2009.
    /// Equivale a ROUND(nValor + 0.00001, 2) em funcoes.prg.
    /// </summary>
    public const decimal AjustePontoFlutuante = 0.00001m;

    /// <summary>
    /// ROUND do VFP arredonda o meio (x.xx5) para longe de zero, não para o par mais próximo.
    /// O padrão do .NET (<see cref="MidpointRounding.ToEven"/>, "banker's rounding") daria 1,12 para 1,125;
    /// o legado e este método dão 1,13.
    /// </summary>
    public static decimal RoundVfp(decimal valor, int casas = 2)
        => Math.Round(valor, casas, MidpointRounding.AwayFromZero);

    /// <summary>Totais de item: ROUND(valor + 0.00001, 2). Ver funcoes.prg / RoundGestcom.</summary>
    public static decimal RoundGestcom(decimal valor, int casas = 2)
        => RoundVfp(valor + AjustePontoFlutuante, casas);

    /// <summary>ICMS: ROUND(base * aliq / 100, 2) sem o +0.00001. Ver spc_calc_icms.prg.</summary>
    public static decimal RoundIcms(decimal valor, int casas = 2)
        => RoundVfp(valor, casas);

    /// <summary>
    /// Valor do desconto de faixa: INT(n * 100) / 100.
    /// Trunca em 2 casas, nunca arredonda para cima (funcoes.prg / Trunca2).
    /// Exemplo: 2,397 vira 2,39 e não 2,40.
    /// </summary>
    public static decimal Trunca2(decimal valor)
        => Math.Truncate(valor * 100m) / 100m;
}
