namespace Gestcom.Domain.Excecoes;

public class CadastroNaoEncontradoException : Exception
{
    public CadastroNaoEncontradoException(string mensagem) : base(mensagem) { }
}
