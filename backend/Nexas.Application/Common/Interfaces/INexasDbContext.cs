using Microsoft.EntityFrameworkCore;
using Nexas.Domain.Entities;

namespace Nexas.Application.Common.Interfaces
{
    public interface INexasDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Course> Courses { get; }
        DbSet<Module> Modules { get; }
        DbSet<Lesson> Lessons { get; }
        DbSet<Enrollment> Enrollments { get; }
        DbSet<Purchase> Purchases { get; }
        DbSet<CourseDomain> CourseDomains { get; }
        DbSet<Teacher> Teachers { get; }
        DbSet<CourseTeacher> CourseTeachers { get; }
        DbSet<BlogPost> BlogPosts { get; }
        DbSet<CourseCategory> CourseCategories { get; }
        DbSet<CourseCourseCategory> CourseCourseCategories { get; }
        DbSet<LessonView> LessonViews { get; }
        DbSet<ForumCategory> ForumCategories { get; }
        DbSet<ForumTopic> ForumTopics { get; }
        DbSet<ForumMessage> ForumMessages { get; }
        DbSet<CourseRate> CourseRates { get; }
        DbSet<Certificate> Certificates { get; }
        DbSet<AppRole> AppRoles { get; }
        DbSet<AppUserRole> AppUserRoles { get; }

        DbSet<FinancialTax> FinancialTaxes { get; }
        DbSet<FinancialClosing> FinancialClosings { get; }
        DbSet<FinancialClosingItem> FinancialClosingItems { get; }
        DbSet<RefundRequest> RefundRequests { get; }
            DbSet<TicketCategory> TicketCategories { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketMessage> TicketMessages { get; }
    DbSet<TicketAttachment> TicketAttachments { get; }
    DbSet<TicketTimeline> TicketTimelines { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

