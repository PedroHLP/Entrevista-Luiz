using Gestcom.Application.Abstracoes;
using Gestcom.Domain.Entidades;
using Gestcom.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Gestcom.Infrastructure.Repositorios;

public sealed class ProdutoRepositorio : IProdutoRepositorio
{
    private readonly GestcomDbContext _db;
    public ProdutoRepositorio(GestcomDbContext db) => _db = db;

    public Task<Produto?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
        => _db.Produtos.FirstOrDefaultAsync(p => p.Codigo == codigo.Trim(), ct);

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken ct = default)
        => await _db.Produtos.AsNoTracking().OrderBy(p => p.Codigo).ToListAsync(ct);

    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default)
        => await _db.Produtos.AddAsync(produto, ct);

    public Task AtualizarAsync(Produto produto, CancellationToken ct = default)
    {
        _db.Produtos.Update(produto);
        return Task.CompletedTask;
    }
}

public sealed class ClienteRepositorio : IClienteRepositorio
{
    private readonly GestcomDbContext _db;
    public ClienteRepositorio(GestcomDbContext db) => _db = db;

    public Task<Cliente?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default)
        => _db.Clientes.FirstOrDefaultAsync(c => c.Codigo == codigo.Trim(), ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default)
        => await _db.Clientes.AsNoTracking().OrderBy(c => c.Codigo).ToListAsync(ct);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default)
        => await _db.Clientes.AddAsync(cliente, ct);
}

public sealed class PedidoRepositorio : IPedidoRepositorio
{
    private readonly GestcomDbContext _db;
    public PedidoRepositorio(GestcomDbContext db) => _db = db;

    public async Task<int> ProximoNumeroAsync(CancellationToken ct = default)
    {
        var max = await _db.Pedidos.Select(p => (int?)p.Numero).MaxAsync(ct);
        return (max ?? 0) + 1;
    }

    public Task<Pedido?> ObterPorNumeroAsync(int numero, CancellationToken ct = default)
        => _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Numero == numero, ct);

    public async Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken ct = default)
        => await _db.Pedidos.AsNoTracking().Include(p => p.Itens).OrderBy(p => p.Numero).ToListAsync(ct);

    public async Task AdicionarAsync(Pedido pedido, CancellationToken ct = default)
        => await _db.Pedidos.AddAsync(pedido, ct);
}

public sealed class SugestaoRepositorio : ISugestaoRepositorio
{
    private readonly GestcomDbContext _db;
    public SugestaoRepositorio(GestcomDbContext db) => _db = db;

    public Task<SugestaoReposicao?> ObterPorProdutoAsync(string codigoProduto, CancellationToken ct = default)
        => _db.SugestoesReposicao.FirstOrDefaultAsync(s => s.CodigoProduto == codigoProduto.Trim(), ct);

    public async Task<IReadOnlyList<SugestaoReposicao>> ListarAsync(CancellationToken ct = default)
        => await _db.SugestoesReposicao.AsNoTracking().OrderBy(s => s.CodigoProduto).ToListAsync(ct);

    public async Task SalvarAsync(SugestaoReposicao sugestao, CancellationToken ct = default)
    {
        var existente = await ObterPorProdutoAsync(sugestao.CodigoProduto, ct);
        if (existente is null)
            await _db.SugestoesReposicao.AddAsync(sugestao, ct);
        else
        {
            existente.QuantidadeAtual = sugestao.QuantidadeAtual;
            existente.QuantidadeMinima = sugestao.QuantidadeMinima;
            existente.QuantidadeSugerida = sugestao.QuantidadeSugerida;
            existente.Data = sugestao.Data;
        }
    }
}

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly GestcomDbContext _db;
    public UnitOfWork(GestcomDbContext db) => _db = db;
    public Task<int> SalvarAlteracoesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
