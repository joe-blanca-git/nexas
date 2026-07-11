using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;

namespace Nexas.Application.Certificates.Queries.ValidateCertificate;

public class ValidateCertificateQueryHandler : IRequestHandler<ValidateCertificateQuery, CertificateDetailDto>
{
    private readonly INexasDbContext _context;

    public ValidateCertificateQueryHandler(INexasDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateDetailDto> Handle(ValidateCertificateQuery request, CancellationToken cancellationToken)
    {
        var certificate = await _context.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
                .ThenInclude(course => course.CourseTeachers)
                    .ThenInclude(ct => ct.Teacher)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ValidationCode == request.ValidationCode, cancellationToken);

        if (certificate == null)
        {
            throw new KeyNotFoundException("Certificado não encontrado ou inválido.");
        }

        var firstTeacher = certificate.Course.CourseTeachers.Select(ct => ct.Teacher).FirstOrDefault();

        return new CertificateDetailDto
        {
            StudentName = certificate.User.FullName ?? "Aluno",
            CourseTitle = certificate.Course.Name,
            TeacherName = firstTeacher?.Name ?? "Equipe Nexas",
            WorkloadHours = certificate.Course.WorkloadHours,
            ValidationCode = certificate.ValidationCode,
            IssuedAt = certificate.IssuedAt
        };
    }
}
