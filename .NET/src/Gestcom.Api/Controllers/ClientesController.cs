using Gestcom.Application.Dtos;
using Gestcom.Application.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Gestcom.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly CatalogoServico _catalogo;
    public ClientesController(CatalogoServico catalogo) => _catalogo = catalogo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Listar(CancellationToken ct)
        => Ok((await _catalogo.ListarClientesAsync(ct)).Select(c => c.ParaDto()));

    [HttpGet("{codigo}")]
    public async Task<ActionResult<ClienteDto>> Obter(string codigo, CancellationToken ct)
        => Ok((await _catalogo.ObterClienteAsync(codigo, ct)).ParaDto());
}
