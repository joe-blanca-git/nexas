using Microsoft.EntityFrameworkCore;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Entities;
using Nexas.Infrastructure.Persistence.Configurations;

namespace Nexas.Infrastructure.Persistence
{
    public class NexasDbContext : DbContext, INexasDbContext
    {
        public NexasDbContext(DbContextOptions<NexasDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<CourseDomain> CourseDomains => Set<CourseDomain>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
        public DbSet<CourseCourseCategory> CourseCourseCategories => Set<CourseCourseCategory>();
        public DbSet<LessonView> LessonViews => Set<LessonView>();
        public DbSet<ForumCategory> ForumCategories => Set<ForumCategory>();
        public DbSet<ForumTopic> ForumTopics => Set<ForumTopic>();
        public DbSet<ForumMessage> ForumMessages => Set<ForumMessage>();
        public DbSet<CourseRate> CourseRates => Set<CourseRate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexasDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
