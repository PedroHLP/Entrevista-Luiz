using Gestcom.Application.Abstracoes;
using Gestcom.Infrastructure.Persistencia;
using Gestcom.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gestcom.Infrastructure;

public static class DependencyInjection
{
    public const string ProviderSqlite = "Sqlite";
    public const string ProviderSqlServer = "SqlServer";
    public const string ProviderInMemory = "InMemory";

    public static IServiceCollection AddGestcomInfra(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? ProviderSqlite;
        var connectionString = configuration["Database:ConnectionString"]
            ?? configuration.GetConnectionString("Gestcom")
            ?? "Data Source=gestcom.db";

        services.AddDbContext<GestcomDbContext>(options =>
        {
            switch (provider)
            {
                case ProviderSqlServer:
                    options.UseSqlServer(connectionString);
                    break;
                case ProviderInMemory:
                    options.UseInMemoryDatabase(connectionString);
                    break;
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

        services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
        services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        services.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
        services.AddScoped<ISugestaoRepositorio, SugestaoRepositorio>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
