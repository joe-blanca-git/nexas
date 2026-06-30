using MediatR;
using Nexas.Application.PortalHome.DTOs;

namespace Nexas.Application.PortalHome.Queries.GetNewsDetail;

public record GetNewsDetailQuery(int Id) : IRequest<NewsDetailDto>;
