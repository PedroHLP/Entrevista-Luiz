using Gestcom.Application.Abstracoes;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Excecoes;

namespace Gestcom.Application.Servicos;

public sealed class CatalogoServico
{
    private readonly IProdutoRepositorio _produtos;
    private readonly IClienteRepositorio _clientes;
    private readonly IUnitOfWork _uow;

    public CatalogoServico(IProdutoRepositorio produtos, IClienteRepositorio clientes, IUnitOfWork uow)
    {
        _produtos = produtos;
        _clientes = clientes;
        _uow = uow;
    }

    public Task<IReadOnlyList<Produto>> ListarProdutosAsync(CancellationToken ct = default)
        => _produtos.ListarAsync(ct);

    public async Task<Produto> ObterProdutoAsync(string codigo, CancellationToken ct = default)
        => await _produtos.ObterPorCodigoAsync(codigo, ct)
           ?? throw new CadastroNaoEncontradoException("Produto inexistente ou inativo.");

    public Task<IReadOnlyList<Cliente>> ListarClientesAsync(CancellationToken ct = default)
        => _clientes.ListarAsync(ct);

    public async Task<Cliente> ObterClienteAsync(string codigo, CancellationToken ct = default)
        => await _clientes.ObterPorCodigoAsync(codigo, ct)
           ?? throw new CadastroNaoEncontradoException("Cliente não encontrado.");

    public async Task SalvarProdutoAsync(Produto produto, CancellationToken ct = default)
    {
        var existente = await _produtos.ObterPorCodigoAsync(produto.Codigo, ct);
        if (existente is null)
            await _produtos.AdicionarAsync(produto, ct);
        else
        {
            existente.Descricao = produto.Descricao;
            existente.Ncm = produto.Ncm;
            existente.Preco = produto.Preco;
            existente.Estoque = produto.Estoque;
            existente.EstoqueMinimo = produto.EstoqueMinimo;
            existente.Ativo = produto.Ativo;
            await _produtos.AtualizarAsync(existente, ct);
        }
        await _uow.SalvarAlteracoesAsync(ct);
    }
}
