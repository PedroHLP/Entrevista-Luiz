using Gestcom.Application.Abstracoes;
using Gestcom.Application.Dtos;
using Gestcom.Domain.Entidades;
using Gestcom.Domain.Excecoes;
using Gestcom.Domain.Servicos;

namespace Gestcom.Application.Servicos;

public sealed class PedidoServico
{
    private readonly IClienteRepositorio _clientes;
    private readonly IProdutoRepositorio _produtos;
    private readonly IPedidoRepositorio _pedidos;
    private readonly ISugestaoRepositorio _sugestoes;
    private readonly IUnitOfWork _uow;
    private readonly CalculadoraPedido _calculadora;

    public PedidoServico(
        IClienteRepositorio clientes,
        IProdutoRepositorio produtos,
        IPedidoRepositorio pedidos,
        ISugestaoRepositorio sugestoes,
        IUnitOfWork uow,
        CalculadoraPedido calculadora)
    {
        _clientes = clientes;
        _produtos = produtos;
        _pedidos = pedidos;
        _sugestoes = sugestoes;
        _uow = uow;
        _calculadora = calculadora;
    }

    public Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken ct = default)
        => _pedidos.ListarAsync(ct);

    public Task<Pedido?> ObterAsync(int numero, CancellationToken ct = default)
        => _pedidos.ObterPorNumeroAsync(numero, ct);

    /// <summary>
    /// Inclui e fecha o pedido, no mesmo fluxo de ped_inclui.prg + trg_baixa_estoque.prg.
    /// A validação de estoque ocorre ANTES do cálculo, comparando cada item com o
    /// saldo atual (como o legado). A quebra de NCM 2202 só entra depois, na baixa.
    /// </summary>
    public async Task<PedidoDto> RegistrarAsync(RegistrarPedidoDto entrada, CancellationToken ct = default)
    {
        if (entrada.Itens is null || entrada.Itens.Count == 0)
            throw new PedidoInvalidoException("Pedido cancelado (nenhum item).");

        var cliente = await _clientes.ObterPorCodigoAsync(entrada.CodigoCliente, ct)
            ?? throw new CadastroNaoEncontradoException("Cliente não encontrado.");

        var itensCalculo = new List<ItemCalculoEntrada>();
        var produtosAlterados = new Dictionary<string, Produto>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in entrada.Itens)
        {
            var produto = await ObterProdutoCacheAsync(produtosAlterados, item.CodigoProduto, ct);
            if (produto is null || !produto.Ativo)
                throw new CadastroNaoEncontradoException("Produto inexistente ou inativo.");
            if (item.Quantidade <= 0)
                throw new PedidoInvalidoException("Quantidade inválida.");

            // Mesmo critério do ped_inclui.prg: compara com o estoque atual,
            // sem acumular reservas entre itens do mesmo produto.
            if (item.Quantidade > produto.Estoque)
                throw new EstoqueInsuficienteException(produto.Codigo, item.Quantidade, produto.Estoque);

            itensCalculo.Add(new ItemCalculoEntrada { Produto = produto, Quantidade = item.Quantidade });
        }

        var numero = await _pedidos.ProximoNumeroAsync(ct);
        var pedido = _calculadora.Calcular(cliente, itensCalculo, DateTime.Today, numero);

        var baixas = new List<ResultadoBaixaEstoque>();
        foreach (var item in pedido.Itens)
        {
            var produto = produtosAlterados[item.CodigoProduto];
            var baixa = ProcessadorBaixaEstoque.Aplicar(produto, item.Quantidade, pedido.Data);
            baixas.Add(baixa);
            await _produtos.AtualizarAsync(produto, ct);
            if (baixa.Sugestao is not null)
                await _sugestoes.SalvarAsync(baixa.Sugestao, ct);
        }

        pedido.Status = "F";
        await _pedidos.AdicionarAsync(pedido, ct);
        await _uow.SalvarAlteracoesAsync(ct);

        return pedido.ParaDto(baixas);
    }

    private async Task<Produto?> ObterProdutoCacheAsync(
        IDictionary<string, Produto> cache, string codigo, CancellationToken ct)
    {
        if (cache.TryGetValue(codigo, out var existente))
            return existente;
        var produto = await _produtos.ObterPorCodigoAsync(codigo, ct);
        if (produto is not null)
            cache[produto.Codigo] = produto;
        return produto;
    }
}
