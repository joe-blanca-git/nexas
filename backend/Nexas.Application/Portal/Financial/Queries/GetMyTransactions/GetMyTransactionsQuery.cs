using MediatR;

namespace Nexas.Application.Portal.Financial.Queries.GetMyTransactions;

public record GetMyTransactionsQuery() : IRequest<List<GetMyTransactionsResponseDto>>;
