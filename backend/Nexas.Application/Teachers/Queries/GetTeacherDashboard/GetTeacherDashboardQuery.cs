using MediatR;

namespace Nexas.Application.Teachers.Queries.GetTeacherDashboard;

public class GetTeacherDashboardQuery : IRequest<TeacherDashboardDto>
{
    public string IdAgivys { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
