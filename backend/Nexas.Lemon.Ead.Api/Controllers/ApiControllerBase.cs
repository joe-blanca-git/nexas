using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Nexas.Lemon.Ead.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
