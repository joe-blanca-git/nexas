using MediatR;

namespace Nexas.Application.Certificates.Queries.ValidateCertificate;

public class ValidateCertificateQuery : IRequest<CertificateDetailDto>
{
    public string ValidationCode { get; set; } = string.Empty;
}
