using Microsoft.OpenApi.Models;
using Nexas.SystemManager.Filters;

namespace Nexas.SystemManager.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.DocumentFilter<HideTwoFactorEndpointFilter>();
                c.OperationFilter<AuthEndpointsDocumentationFilter>();
                c.SchemaFilter<HideLoginTwoFactorFieldsFilter>();
                // POST /api/v1/auth/register tem 2 endpoints válidos no roteamento (o nosso e o do
                // MapIdentityApi, desempatados em runtime via Order) — o Swashbuckle não entende essa
                // prioridade e trava ao gerar o doc, então instruímos ele a manter só o primeiro
                // (o nosso, registrado antes do MapIdentityApi).
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Authenticator",
                    Version = "v1",
                    Description = "API de gerenciamento de Applications e Identity da plataforma Nexas.",
                    Contact = new OpenApiContact
                    {
                        Name = "Nexas Support",
                        Email = "suporte@nexas.com.br"
                    }
                });

                var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
                if (File.Exists(apiXmlPath))
                {
                    c.IncludeXmlComments(apiXmlPath);
                }

                var appXmlFile = "Nexas.Application.xml";
                var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
                if (File.Exists(appXmlPath))
                {
                    c.IncludeXmlComments(appXmlPath);
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            return services;
        }
    }
}
