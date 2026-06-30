using Microsoft.AspNetCore.Mvc;
using Nexas.Application.PortalHome.Queries.GetHome;
using Nexas.Application.PortalHome.Queries.GetNewsDetail;
using Nexas.Application.PortalHome.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Nexas.Api.Controllers;

[Authorize]
[Route("api/v1/portal/home")]
[Tags("Portal Pan - Home")]
public class PortalHomeController : ApiControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Carregar os dados principais da Home do Portal Pan", Description = "Retorna o último curso cadastrado e as 7 notícias mais recentes.")]
    [ProducesResponseType(typeof(PortalHomeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHome()
    {
        var result = await Mediator.Send(new GetHomeQuery());
        return Ok(result);
    }

    [HttpGet("news/{id}")]
    [SwaggerOperation(Summary = "Carregar os detalhes de uma notícia", Description = "Busca os detalhes completos de uma publicação do blog usando o seu ID.")]
    [ProducesResponseType(typeof(NewsDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNewsDetail(int id)
    {
        var result = await Mediator.Send(new GetNewsDetailQuery(id));
        return Ok(result);
    }
}
