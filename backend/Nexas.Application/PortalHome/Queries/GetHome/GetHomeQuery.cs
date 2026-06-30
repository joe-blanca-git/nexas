using MediatR;
using Nexas.Application.PortalHome.DTOs;

namespace Nexas.Application.PortalHome.Queries.GetHome;

public record GetHomeQuery : IRequest<PortalHomeDto>;
