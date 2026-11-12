using Gestcom.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Gestcom.Infrastructure.Persistencia;

public class GestcomDbContext : DbContext
{
    public GestcomDbContext(DbContextOptions<GestcomDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<SugestaoReposicao> SugestoesReposicao => Set<SugestaoReposicao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestcomDbContext).Assembly);
    }
}
