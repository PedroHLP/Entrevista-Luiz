namespace Gestcom.Domain.Entidades;

public class Cliente
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoCliente Tipo { get; set; }
    public string Uf { get; set; } = string.Empty;
}
