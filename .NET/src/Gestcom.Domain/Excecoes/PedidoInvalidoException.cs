namespace Gestcom.Domain.Excecoes;

public class PedidoInvalidoException : Exception
{
    public PedidoInvalidoException(string mensagem) : base(mensagem) { }
}
