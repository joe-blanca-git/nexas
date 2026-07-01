using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

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
            throw new ArgumentException("CPF é obrigatório");

        var externalId = _currentUserService.ExternalId;
        if (externalId == null)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.ExternalId == externalId, cancellationToken)
            ?? throw new InvalidOperationException($"Usuário {externalId} não encontrado.");

        // Update CPF if necessary
        if (string.IsNullOrWhiteSpace(user.CpfCnpj) || user.CpfCnpj != dto.Cpf)
        {
            user.UpdateProfile(user.FullName ?? string.Empty, dto.Cpf);
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Get or Create Asaas Customer
        if (string.IsNullOrWhiteSpace(user.AsaasCustomerId))
        {
            var customerId = await _asaasService.CreateCustomerAsync(user, cancellationToken);
            user.UpdateAsaasCustomerId(customerId);
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
