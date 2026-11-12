using Gestcom.Application.Servicos;
using Gestcom.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace Gestcom.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGestcomApplication(this IServiceCollection services)
    {
        services.AddSingleton<CalculadoraPedido>();
        services.AddScoped<PedidoServico>();
        services.AddScoped<CatalogoServico>();
        services.AddScoped<ReposicaoServico>();
        return services;
    }
}
