using Microsoft.EntityFrameworkCore;
using Nexas.SystemManager.Infrastructure;

namespace Nexas.SystemManager.Filters
{
    /// <summary>
    /// Resolve a Application dona da requisição a partir do header "X-Api-Key" (a chave gerada
    /// em ApplicationsController.GenerateApiKey) — usado pelos endpoints de autenticação dos
    /// usuários finais de cada Application (AppEndUserEndpoints), que não passam pelo esquema
    /// de autenticação do portal (Identity.Bearer). Guarda a Application resolvida em
    /// HttpContext.Items["CurrentApp"] para os handlers lerem.
    /// </summary>
    public class RequireApiKeyFilter : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;

            if (!http.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyValues) || string.IsNullOrWhiteSpace(apiKeyValues))
            {
                return Results.Unauthorized();
            }

            var apiKey = apiKeyValues.ToString();
            var dbContext = http.RequestServices.GetRequiredService<SystemManagerDbContext>();
            var app = await dbContext.Applications.FirstOrDefaultAsync(a => a.ApiKey == apiKey);

            if (app == null)
            {
                return Results.Unauthorized();
            }

            http.Items["CurrentApp"] = app;
            return await next(context);
        }
    }
}
