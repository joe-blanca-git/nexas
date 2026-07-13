using MediatR;
using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Certificates.DTOs;

namespace Nexas.Application.Certificates.Queries.GetMyCertificates;

public class GetMyCertificatesQueryHandler : IRequestHandler<GetMyCertificatesQuery, MyCertificatesResponseDto>
{
    private readonly INexasDbContext _context;
    private readonly IUserContextService _userContext;

    public GetMyCertificatesQueryHandler(INexasDbContext context, IUserContextService userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<MyCertificatesResponseDto> Handle(GetMyCertificatesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userContext.GetCurrentUserAsync();
        if (user == null)
            throw new UnauthorizedAccessException("Usuário não autenticado.");

        var userId = user.Id;

        var certificates = await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssuedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = new MyCertificatesResponseDto
        {
            TotalCertificates = certificates.Count(),
            TotalHours = certificates.Sum(c => c.Course.WorkloadHours),
            Certificates = certificates.Select(c => new MyCertificateDto
            {
                Hash = c.ValidationCode,
                CourseName = c.Course.Name,
                Workload = c.Course.WorkloadHours,
                CompletionDate = c.IssuedAt.ToString("dd/MM/yyyy"),
                Status = "Concluído",
                CoverImage = c.Course.ImgCoverLink
            }).ToList()
        };

        return response;
    }
}
