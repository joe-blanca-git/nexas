using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MeController : ControllerBase
    {
        private readonly IUserContextService _userContextService;

        public MeController(IUserContextService userContextService)
        {
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContextService.GetCurrentUserAsync();
            
            return Ok(new
            {
                user_id = user.Id,
                external_id = user.ExternalId,
                email = user.Email,
                created_at = user.CreatedAt
            });
        }
    }
}
