namespace Nexas.Application.Teachers.Queries.GetTeacherDashboard;

public class TeacherDashboardDto
{
    public int TotalCoursesSold { get; set; }
    public decimal SalesGrowthPercentage { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueGrowthPercentage { get; set; }
    public decimal CurrentMonthRevenue { get; set; }
    public int TotalActiveStudents { get; set; }
    public decimal[] MonthlyRevenueCurrentYear { get; set; } = new decimal[12];
    public decimal TodayRevenue { get; set; }
    public int NewStudentsThisMonth { get; set; }
    public int TotalActivePublishedCourses { get; set; }
    public int TotalPublishedClassesOfActiveCourses { get; set; }
    public int TotalOpenForumsWithoutReply { get; set; }
    public decimal AverageCourseRating { get; set; }
    public List<DashboardForumDto> Last5Forums { get; set; } = new List<DashboardForumDto>();
}
