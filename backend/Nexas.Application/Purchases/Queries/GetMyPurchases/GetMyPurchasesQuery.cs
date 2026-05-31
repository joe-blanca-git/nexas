using MediatR;
using System.Collections.Generic;

namespace Nexas.Application.Purchases.Queries.GetMyPurchases;

public record GetMyPurchasesQuery() : IRequest<List<UserPurchaseDto>>;
