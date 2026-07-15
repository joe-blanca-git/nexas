using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexas.Application.Common.Interfaces;
using Nexas.Infrastructure.Persistence;
using Nexas.Infrastructure.ExternalServices.Asaas;
using Nexas.Infrastructure.Services;

namespace Nexas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

            services.AddDbContext<NexasDbContext>(options =>
                options.UseMySql(connectionString, serverVersion,
                b => b.MigrationsAssembly(typeof(NexasDbContext).Assembly.FullName)));

            services.AddScoped<INexasDbContext>(provider => provider.GetRequiredService<NexasDbContext>());

            services.AddHttpClient<IAsaasService, AsaasService>(client =>
            {
                var baseUrl = configuration["Asaas:BaseUrl"] ?? "https://sandbox.asaas.com/api/v3/";
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Nexas.Backend/1.0");
            });
            services.AddScoped<ICloudflareStorageService, CloudflareStorageService>();
            
            services.AddHttpClient<IBunnyNetService, BunnyNetService>(client =>
            {
                client.BaseAddress = new Uri("https://api.bunny.net/");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });
                                
            return services;
        }
    }
}
