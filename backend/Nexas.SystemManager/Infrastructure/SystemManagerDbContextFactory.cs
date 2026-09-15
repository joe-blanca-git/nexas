using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Nexas.SystemManager.Infrastructure
{
    public class SystemManagerDbContextFactory : IDesignTimeDbContextFactory<SystemManagerDbContext>
    {
        public SystemManagerDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<SystemManagerDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseMySql(connectionString, ServerVersion.Parse("8.0.0-mysql"));

            return new SystemManagerDbContext(builder.Options);
        }
    }
}
