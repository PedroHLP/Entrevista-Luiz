using Gestcom.Application.Dtos;
using Gestcom.Application.Servicos;
using Gestcom.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Gestcom.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly CatalogoServico _catalogo;
    public ProdutosController(CatalogoServico catalogo) => _catalogo = catalogo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProdutoDto>>> Listar(CancellationToken ct)
        => Ok((await _catalogo.ListarProdutosAsync(ct)).Select(p => p.ParaDto()));

    [HttpGet("{codigo}")]
    public async Task<ActionResult<ProdutoDto>> Obter(string codigo, CancellationToken ct)
        => Ok((await _catalogo.ObterProdutoAsync(codigo, ct)).ParaDto());

    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Salvar([FromBody] ProdutoDto dto, CancellationToken ct)
    {
        var produto = new Produto
        {
            Codigo = dto.Codigo.Trim(),
            Descricao = dto.Descricao.Trim(),
            Ncm = dto.Ncm.Trim(),
            Preco = dto.Preco,
            Estoque = dto.Estoque,
            EstoqueMinimo = dto.EstoqueMinimo,
            Ativo = dto.Ativo
        };
        await _catalogo.SalvarProdutoAsync(produto, ct);
        return Ok(produto.ParaDto());
    }
}
