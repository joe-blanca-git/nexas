using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nexas.SystemManager.Filters
{
    public class HideTwoFactorEndpointFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var pathsToRemove = swaggerDoc.Paths
                .Where(p => p.Key.Contains("/manage/2fa", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.Key)
                .ToList();

            foreach (var path in pathsToRemove)
            {
                swaggerDoc.Paths.Remove(path);
            }
        }
    }
}
