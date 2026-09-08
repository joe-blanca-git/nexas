using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexas.Application.Users.Queries.GetUsers;

namespace Nexas.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Lista todos os usuários.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        return await Mediator.Send(new GetUsersQuery());
    }
}
