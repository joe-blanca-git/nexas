using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Lemon.Ead.Api.Controllers;

[Authorize]
[Route("api/v1/[controller]")]
public class TestAuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public TestAuthController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new 
        { 
            Claims = claims,
            ExtId = _currentUserService.ExternalId,
            Email = _currentUserService.Email,
            FullName = _currentUserService.FullName
        });
    }
}
