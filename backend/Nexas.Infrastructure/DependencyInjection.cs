using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexas.Application.Common.Interfaces;
using Nexas.Infrastructure.Persistence;

namespace Nexas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<NexasDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                    b => b.MigrationsAssembly(typeof(NexasDbContext).Assembly.FullName)));

            services.AddScoped<INexasDbContext>(provider => provider.GetRequiredService<NexasDbContext>());
            
            return services;
        }
    }
}
