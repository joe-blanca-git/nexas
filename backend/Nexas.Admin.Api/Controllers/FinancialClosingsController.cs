using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Nexas.Application.FinancialClosings.Queries;

namespace Nexas.Admin.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class FinancialClosingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinancialClosingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtém o resumo financeiro global do professor logado (Dashboard).
        /// </summary>
        [HttpGet("Summary")]
        [SwaggerOperation(Summary = "Resumo financeiro", Description = "Retorna os totais disponíveis, pendentes e já sacados do professor autenticado.")]
        [ProducesResponseType(typeof(FinancialDashboardSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _mediator.Send(new GetFinancialDashboardSummaryQuery());
            return Ok(result);
        }

        /// <summary>
        /// Simula um fechamento com base nas vendas elegíveis (mais antigas que 7 dias).
        /// </summary>
        [HttpGet("Simulate")]
        [SwaggerOperation(Summary = "Simula fechamento de vendas", Description = "Busca as vendas elegíveis e calcula os repasses detalhados sem salvar nada no banco de dados.")]
        [ProducesResponseType(typeof(FinancialClosingSimulationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Simulate()
        {
            var result = await _mediator.Send(new SimulateProfessorClosingQuery());
            return Ok(result);
        }

        /// <summary>
        /// Obtém o histórico de faturamentos/fechamentos.
        /// </summary>
        [HttpGet]
        [SwaggerOperation(Summary = "Histórico de Fechamentos", Description = "Retorna todos os fechamentos gerados. Administradores podem filtrar por professor.")]
        [ProducesResponseType(typeof(List<FinancialClosingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistory([FromQuery] int? teacherId)
        {
            var result = await _mediator.Send(new GetClosingsHistoryQuery(teacherId));
            return Ok(result);
        }

        /// <summary>
        /// Processa o faturamento das vendas elegíveis para um professor.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [SwaggerOperation(Summary = "Gera Faturamento", Description = "Congela as taxas vigentes e gera o repasse para o professor, movendo as vendas para o status de 'Pendente de Pagamento'.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ProcessClosing([FromBody] Nexas.Application.FinancialClosings.Commands.ProcessProfessorClosingCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Created("", new { message = result.Message, id = result.ClosingId });
        }

        /// <summary>
        /// Realiza a baixa do pagamento (com anexo PIX) do faturamento.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/Pay")]
        [SwaggerOperation(Summary = "Efetua Pagamento do Faturamento", Description = "Anexa o comprovante PIX e o ID da transação Asaas para finalizar o repasse.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PayClosing(int id, [FromForm] string asaasTransferId, IFormFile receiptFile)
        {
            var command = new Nexas.Application.FinancialClosings.Commands.PayFinancialClosingCommand
            {
                ClosingId = id,
                AsaasTransferId = asaasTransferId,
                FileName = receiptFile?.FileName,
                ContentType = receiptFile?.ContentType,
                FileStream = receiptFile?.OpenReadStream()
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
