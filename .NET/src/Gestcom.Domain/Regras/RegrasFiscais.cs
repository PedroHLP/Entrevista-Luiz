namespace Gestcom.Domain.Regras;

/// <summary>
/// CFOP e alíquota de ICMS. Fonte: spc_calc_icms.prg.
/// A redução para NCM 1905 (padaria, 7%) está no legado como bloco "circular 14/2012"
/// e não aparece no manual do usuário.
/// </summary>
public static class RegrasFiscais
{
    public const string UfEmpresa = "SP";
    public const string CfopDentroEstado = "5102";
    public const string CfopInterestadual = "6102";
    public const decimal AliquotaInterna = 18m;
    public const decimal AliquotaInterestadual = 12m;
    public const decimal AliquotaPadaria = 7m;
    public const string PrefixoNcmPadaria = "1905";

    public static bool MesmaUf(string ufCliente)
        => string.Equals((ufCliente ?? string.Empty).Trim(), UfEmpresa, StringComparison.OrdinalIgnoreCase);

    public static string Cfop(string ufCliente)
        => MesmaUf(ufCliente) ? CfopDentroEstado : CfopInterestadual;

    public static bool TemAliquotaReduzidaPadaria(string ncm)
        => (ncm ?? string.Empty).Trim().StartsWith(PrefixoNcmPadaria, StringComparison.Ordinal);

    public static decimal AliquotaIcms(string ncm, string ufCliente)
    {
        if (TemAliquotaReduzidaPadaria(ncm))
            return AliquotaPadaria;

        return MesmaUf(ufCliente) ? AliquotaInterna : AliquotaInterestadual;
    }
}
