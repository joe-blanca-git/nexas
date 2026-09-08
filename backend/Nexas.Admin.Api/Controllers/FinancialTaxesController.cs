using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Nexas.Application.FinancialTaxes.Commands;
using Nexas.Application.FinancialTaxes.Queries;

namespace Nexas.Admin.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FinancialTaxesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinancialTaxesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtém todas as taxas financeiras cadastradas.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Lista todas as taxas financeiras", Description = "Retorna a lista de taxas configuradas para plataforma, banco ou recebedores.")]
        [ProducesResponseType(typeof(List<FinancialTaxDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetFinancialTaxesQuery());
            return Ok(result);
        }

        /// <summary>
        /// Cria uma nova taxa financeira.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Cria uma nova taxa", Description = "Cadastra uma nova taxa financeira.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateFinancialTaxCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Created("", new { message = result.Message });
        }

        /// <summary>
        /// Alterna o status (Ativo/Inativo) de uma taxa financeira.
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [SwaggerOperation(Summary = "Alterna status da taxa", Description = "Ativa ou inativa uma taxa.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _mediator.Send(new ToggleFinancialTaxStatusCommand(id));
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
