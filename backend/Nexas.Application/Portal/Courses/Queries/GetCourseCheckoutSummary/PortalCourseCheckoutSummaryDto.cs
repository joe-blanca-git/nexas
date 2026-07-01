namespace Nexas.Application.Portal.Courses.Queries.GetCourseCheckoutSummary;

public record PortalCourseCheckoutSummaryDto(
    int Id,
    string Title,
    string? ImgCoverLink,
    string? Description,
    int TotalLessons,
    int TotalHours,
    string Category,
    decimal PriceSingle,
    decimal PriceSubscription
);
