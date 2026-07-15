using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Nexas.Landing.Api.Controllers;

[ApiController]
[Route("v1/api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
