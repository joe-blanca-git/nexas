using MediatR;
using Nexas.Application.Certificates.DTOs;

namespace Nexas.Application.Certificates.Queries.GetMyCertificates;

public class GetMyCertificatesQuery : IRequest<MyCertificatesResponseDto>
{
}
