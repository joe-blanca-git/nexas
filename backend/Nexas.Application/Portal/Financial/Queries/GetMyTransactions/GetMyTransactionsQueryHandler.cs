using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;

namespace Nexas.Application.Portal.Financial.Queries.GetMyTransactions;

public class GetMyTransactionsQueryHandler : IRequestHandler<GetMyTransactionsQuery, List<GetMyTransactionsResponseDto>>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContextService;

    public GetMyTransactionsQueryHandler(INexasDbContext context, IUserContextService userContextService)
    {
        _context = context;
        _userContextService = userContextService;
    }

    public async Task<List<GetMyTransactionsResponseDto>> Handle(GetMyTransactionsQuery request, CancellationToken cancellationToken)
    {
        var currentUser = await _userContextService.GetCurrentUserAsync();

        var purchases = await _context.Purchases
            .Include(p => p.Course)
            .AsNoTracking()
            .Where(p => p.UserId == currentUser.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return purchases.Select(p => new GetMyTransactionsResponseDto
        {
            Id = p.Id,
            Name = $"Compra do curso {p.Course?.Name}",
            PaymentMethod = p.PaymentMethod,
            Status = p.Status.ToString(), // Pode mapear para português se desejar depois
            PaymentDate = p.CreatedAt
        }).ToList();
    }
}
