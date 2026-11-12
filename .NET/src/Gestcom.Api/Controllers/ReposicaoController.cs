using Gestcom.Application.Dtos;
using Gestcom.Application.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Gestcom.Api.Controllers;

[ApiController]
[Route("api/reposicao")]
public class ReposicaoController : ControllerBase
{
    private readonly ReposicaoServico _reposicao;
    public ReposicaoController(ReposicaoServico reposicao) => _reposicao = reposicao;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SugestaoReposicaoDto>>> Listar(CancellationToken ct)
        => Ok((await _reposicao.ListarAsync(ct)).Select(s => s.ParaDto()));
}
