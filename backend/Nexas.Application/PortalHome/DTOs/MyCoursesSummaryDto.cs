namespace Nexas.Application.PortalHome.DTOs;

public record MyCoursesSummaryDto(
    int TotalCourses,
    int CompletedCourses,
    int CoursesInProgress,
    int OverallProgress
);
