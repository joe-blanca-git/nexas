using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexas.Application.Common.Interfaces;
using Nexas.Domain.Enums;

namespace Nexas.Infrastructure.Jobs
{
    public class OrphanVideoCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrphanVideoCleanupBackgroundService> _logger;

        public OrphanVideoCleanupBackgroundService(IServiceProvider serviceProvider, ILogger<OrphanVideoCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrphanVideoCleanupBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOrphanVideosAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during OrphanVideoCleanupBackgroundService.");
                }

                // Wait 1 hour before next cleanup check
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CleanupOrphanVideosAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<INexasDbContext>();
            var bunnyVideoService = scope.ServiceProvider.GetRequiredService<IBunnyVideoService>();

            // Find lessons stuck in PendingUpload for more than 24 hours
            var cutoffTime = DateTime.UtcNow.AddHours(-24);
            var orphanLessons = await context.Lessons
                .Include(l => l.Module)
                .ThenInclude(m => m.Course)
                .Where(l => l.Status == LessonStatus.PendingUpload 
                            && l.CreatedAt < cutoffTime 
                            && !string.IsNullOrEmpty(l.BunnyVideoId))
                .ToListAsync(cancellationToken);

            if (!orphanLessons.Any())
                return;

            _logger.LogInformation($"Found {orphanLessons.Count} orphan videos to cleanup.");

            foreach (var lesson in orphanLessons)
            {
                try
                {
                    // Attempt to delete from Bunny Stream
                    // Note: We need LibraryId which is typically linked to Course or a Global config.
                    // Assuming Course has BunnyLibraryId as string.
                    if (int.TryParse(lesson.Module.Course.BunnyLibraryId, out int libraryId))
                    {
                        var deleted = await bunnyVideoService.DeleteVideoAsync(libraryId, lesson.BunnyVideoId!, cancellationToken);
                        if (deleted)
                        {
                            _logger.LogInformation($"Successfully deleted orphan video {lesson.BunnyVideoId} from Bunny.");
                        }
                    }

                    // Reset lesson to Draft
                    lesson.Status = LessonStatus.Draft;
                    lesson.BunnyVideoId = null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to cleanup orphan video for Lesson {lesson.Id}");
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
