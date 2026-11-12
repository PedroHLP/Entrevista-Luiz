using Gestcom.Application.Dtos;
using Gestcom.Application.Servicos;
using Gestcom.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Gestcom.Api;

/// <summary>
/// Pedidos de exemplo para a demonstração: passam pelo PedidoServico, então totais,
/// baixa de estoque e sugestões de reposição saem das mesmas regras do legado.
/// Não usa os produtos 0004 (refrigerante) e 0006 (água), reservados ao roteiro ao vivo.
/// Desligue com "Demo:PedidosExemplo": false.
/// </summary>
public static class CargaDemonstracao
{
    private static readonly RegistrarPedidoDto[] Pedidos =
    [
        new("0001", [new("0001", 12m), new("0002", 18m)]),
        new("0002", [new("0003", 16m), new("0007", 20m)]),
        new("0004", [new("0005", 2m), new("0001", 1m)]),
        new("0005", [new("0001", 6m), new("0007", 6m)]),
        new("0003", [new("0003", 2m), new("0007", 3m)]),
    ];

    public static async Task GarantirAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<GestcomDbContext>();
        if (await db.Pedidos.AnyAsync(ct))
            return;

        var servico = services.GetRequiredService<PedidoServico>();
        foreach (var pedido in Pedidos)
            await servico.RegistrarAsync(pedido, ct);
    }
}
