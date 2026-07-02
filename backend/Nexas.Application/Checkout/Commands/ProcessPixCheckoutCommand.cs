using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Domain.Enums;

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

        // VERIFICAÇÕES DE REGRAS DE NEGÓCIO (FLUXO 4 e 5)
        if (dto.TipoCompra == "AVULSO")
        {
            var jaPossui = await _context.Purchases
                .AnyAsync(p => p.UserId == user.Id && p.CourseId == dto.CursoId && p.Status == PurchaseStatus.Approved, cancellationToken);
            if (jaPossui)
                throw new Exception("Você já possui acesso a este curso.");

            var pendente = await _context.Purchases
                .Where(p => p.UserId == user.Id && p.CourseId == dto.CursoId && p.Status == PurchaseStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendente != null)
            {
                if (!string.IsNullOrEmpty(pendente.AsaasPaymentId))
                {
                    try
                    {
                        var qrData = await _asaasService.GetPixQrCodeAsync(pendente.AsaasPaymentId, cancellationToken);
                        return new CheckoutPixResponseDto
                        {
                            Sucesso = true,
                            CobrancaId = pendente.AsaasPaymentId,
                            PixCopiaECola = qrData.Payload,
                            QrCode = qrData.EncodedImage
                        };
                    }
                    catch
                    {
                        pendente.Cancel();
                        await _context.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
        else if (dto.TipoCompra == "ANUAL")
        {
            var jaPossui = await _context.Subscriptions
                .AnyAsync(s => s.UserId == user.Id && s.Status == Nexas.Domain.Enums.SubscriptionStatus.Active, cancellationToken);
            if (jaPossui)
                throw new Exception("Você já possui uma assinatura ativa.");

            var pendente = await _context.SubscriptionPayments
                .Include(sp => sp.Subscription)
                .Where(sp => sp.Subscription.UserId == user.Id && sp.Status == Nexas.Domain.Enums.SubscriptionPaymentStatus.Pending)
                .OrderByDescending(sp => sp.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendente != null && !string.IsNullOrEmpty(pendente.AsaasPaymentId))
            {
                try
                {
                    var qrData = await _asaasService.GetPixQrCodeAsync(pendente.AsaasPaymentId, cancellationToken);
                    return new CheckoutPixResponseDto
                    {
                        Sucesso = true,
                        CobrancaId = pendente.AsaasPaymentId,
                        PixCopiaECola = qrData.Payload,
                        QrCode = qrData.EncodedImage
                    };
                }
                catch
                {
                    pendente.Cancel();
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        var description = $"{(dto.TipoCompra == "ANUAL" ? "Assinatura Anual" : "Curso Avulso")} - Curso ID: {dto.CursoId}";

        // Create Payment in Asaas
        var cobrancaId = await _asaasService.CreatePixPaymentAsync(user.AsaasCustomerId, dto.Valor, description, cancellationToken);

        // GRAVAR A PENDÊNCIA NO BANCO PARA O GETPENDENCIAS FUNCIONAR
        if (dto.TipoCompra == "AVULSO")
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CursoId, cancellationToken)
                ?? throw new Exception("Curso não encontrado.");

            var purchase = Purchase.Create(user.Id, course.Id, dto.Valor, "PIX");
            purchase.UpdateAsaasPaymentId(cobrancaId);
            _context.Purchases.Add(purchase);
        }
        else if (dto.TipoCompra == "ANUAL")
        {
            var subscription = Subscription.Create(
                user.Id,
                "Assinatura Anual",
                DateTime.UtcNow,
                null,
                true,
                Nexas.Domain.Enums.SubscriptionStatus.Pending,
                null); 
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync(cancellationToken);

            var subPayment = SubscriptionPayment.Create(
                subscription.Id, 
                dto.Valor,
                DateTime.UtcNow,
                Nexas.Domain.Enums.SubscriptionPaymentStatus.Pending,
                cobrancaId);
            
            _context.SubscriptionPayments.Add(subPayment);
        }

        await _context.SaveChangesAsync(cancellationToken);

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
