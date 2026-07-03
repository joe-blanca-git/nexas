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

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
