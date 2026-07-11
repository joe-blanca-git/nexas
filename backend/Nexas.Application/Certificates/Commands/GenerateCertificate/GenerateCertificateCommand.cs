using MediatR;

namespace Nexas.Application.Certificates.Commands.GenerateCertificate;

public class GenerateCertificateCommand : IRequest<string>
{
    public int CourseId { get; set; }
}
