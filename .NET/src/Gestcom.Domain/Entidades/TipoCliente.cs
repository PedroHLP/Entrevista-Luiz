namespace Gestcom.Domain.Entidades;

/// <summary>
/// Espelha o campo CLIENTES.TIPO do legado (C/V/F).
/// </summary>
public enum TipoCliente
{
    Consumidor = 'C',
    Varejista = 'V',
    Funcionario = 'F'
}

public static class TipoClienteExtensoes
{
    public static TipoCliente DeCodigo(string codigo)
    {
        return (codigo ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "C" => TipoCliente.Consumidor,
            "V" => TipoCliente.Varejista,
            "F" => TipoCliente.Funcionario,
            _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, "Tipo de cliente desconhecido (esperado C, V ou F).")
        };
    }

    public static string ParaCodigo(this TipoCliente tipo) => ((char)tipo).ToString();
}
