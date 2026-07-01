using MediatR;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Application.Common.Exceptions;

namespace Nexas.Application.Checkout.Commands;

public class ProcessPixCheckoutCommand : IRequest<CheckoutPixResponseDto>
{
    public CheckoutPixRequestDto Request { get; set; } = null!;
}

public class ProcessPixCheckoutCommandHandler : IRequestHandler<ProcessPixCheckoutCommand, CheckoutPixResponseDto>
{
    private readonly IAsaasService _asaasService;
    private readonly ICurrentUserService _currentUserService;
    private readonly INexasDbContext _context;

    public ProcessPixCheckoutCommandHandler(
        IAsaasService asaasService,
        ICurrentUserService currentUserService,
        INexasDbContext context)
    {
        _asaasService = asaasService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<CheckoutPixResponseDto> Handle(ProcessPixCheckoutCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;

        // Validations
        if (string.IsNullOrWhiteSpace(dto.Cpf))
            throw new ValidationException(new Dictionary<string, string[]> { { "Cpf", new[] { "CPF é obrigatório" } } });

        var userId = _currentUserService.UserId;
        if (userId == null)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        // Update CPF if necessary
        if (string.IsNullOrWhiteSpace(user.CpfCnpj) || user.CpfCnpj != dto.Cpf)
        {
            user.CpfCnpj = dto.Cpf;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Get or Create Asaas Customer
        if (string.IsNullOrWhiteSpace(user.AsaasCustomerId))
        {
            user.AsaasCustomerId = await _asaasService.CreateCustomerAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var description = $"{(dto.TipoCompra == "ANUAL" ? "Assinatura Anual" : "Curso Avulso")} - Curso ID: {dto.CursoId}";

        // 1. Create Payment in Asaas
        var cobrancaId = await _asaasService.CreatePixPaymentAsync(user.AsaasCustomerId, dto.Valor, description, cancellationToken);

        // 2. Get QR Code details
        var qrCodeData = await _asaasService.GetPixQrCodeAsync(cobrancaId, cancellationToken);

        return new CheckoutPixResponseDto
        {
            Sucesso = true,
            CobrancaId = cobrancaId,
            PixCopiaECola = qrCodeData.Payload,
            QrCode = qrCodeData.EncodedImage
        };
    }
}
