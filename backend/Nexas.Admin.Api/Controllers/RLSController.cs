using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Nexas.Application.RLS.Commands.AssignRole;
using Nexas.Application.RLS.Commands.RemoveRole;
using Nexas.Application.RLS.Queries.GetRoles;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexas.Admin.Api.Controllers;

[Authorize]
public class RLSController : ApiControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Obter Roles", Description = "Retorna a lista de todas as roles cadastradas.")]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles()
    {
        var result = await Mediator.Send(new GetRolesQuery());
        return Ok(result);
    }

    [HttpPost("assignrole")]
    [SwaggerOperation(Summary = "Vincular Role", Description = "Vincula um usuário a uma role.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("removerole")]
    [SwaggerOperation(Summary = "Remover Role", Description = "Remove uma role de um usuário.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
