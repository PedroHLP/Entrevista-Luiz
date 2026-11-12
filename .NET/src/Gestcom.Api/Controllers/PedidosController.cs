using Gestcom.Application.Dtos;
using Gestcom.Application.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Gestcom.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidoServico _pedidos;
    public PedidosController(PedidoServico pedidos) => _pedidos = pedidos;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PedidoDto>>> Listar(CancellationToken ct)
        => Ok((await _pedidos.ListarAsync(ct)).Select(p => p.ParaDto()));

    [HttpGet("{numero:int}")]
    public async Task<ActionResult<PedidoDto>> Obter(int numero, CancellationToken ct)
    {
        var pedido = await _pedidos.ObterAsync(numero, ct);
        return pedido is null ? NotFound(new { erro = "Pedido não encontrado." }) : Ok(pedido.ParaDto());
    }

    /// <summary>
    /// Inclui e fecha o pedido, baixando estoque (com a quebra operacional do legado) e gerando sugestão de reposição.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Registrar([FromBody] RegistrarPedidoDto entrada, CancellationToken ct)
    {
        var pedido = await _pedidos.RegistrarAsync(entrada, ct);
        return CreatedAtAction(nameof(Obter), new { numero = pedido.Numero }, pedido);
    }
}
