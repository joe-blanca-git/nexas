using Microsoft.EntityFrameworkCore;
using Nexas.Application;
using Nexas.Application.Common.Interfaces;
using Nexas.Infrastructure;
using Nexas.Infrastructure.Persistence;
using Nexas.Infrastructure.Configuration;
using Nexas.SystemManager.Domain.Entities;
using Nexas.SystemManager.Endpoints;
using Nexas.SystemManager.Extensions;
using Nexas.SystemManager.Hubs;
using Nexas.SystemManager.Middlewares;
using Nexas.SystemManager.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var agivysEmail = builder.Configuration["Agivys:Email"];
var agivysPassword = builder.Configuration["Agivys:Password"];
if (!string.IsNullOrEmpty(agivysEmail) && !string.IsNullOrEmpty(agivysPassword))
{
    builder.Configuration.AddAgivysConfiguration(agivysEmail, agivysPassword);
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
                !string.IsNullOrEmpty(origin) && (
                    new Uri(origin).Host.EndsWith("portalnexas.com.br") || 
                    new Uri(origin).Host == "localhost" ||
                    new Uri(origin).Host == "127.0.0.1"
                ))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ASP.NET Core Identity (Provedor de Identidade Isolado) — único esquema de autenticação
// deste projeto. Não registramos o JwtBearer próprio da Nexas aqui (diferente das outras
// APIs): esse esquema emite tokens opacos incompatíveis com o JWT usado pelas demais APIs,
// e ter os dois esquemas registrados ao mesmo tempo fazia o token do /login (Identity) não
// autenticar em nada, porque o esquema padrão continuava sendo o JwtBearer.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<Nexas.SystemManager.Infrastructure.SystemManagerDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentityApiEndpoints<NexasUser>()
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
    .AddEntityFrameworkStores<Nexas.SystemManager.Infrastructure.SystemManagerDbContext>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPaymentEventPublisher, PaymentEventPublisher>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 120,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = 429;
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new
                {
                    PropertyName = e.Key,
                    ErrorMessage = e.Value.Errors.First().ErrorMessage
                }).ToList();

            var response = new
            {
                StatusCode = 400,
                Message = "Um ou mais erros de validação ocorreram na sua requisição. Verifique o formato dos dados enviados.",
                Errors = errors,
                StackTrace = (string)null
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerSetup();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authenticator v1");
});

// Automatically apply pending EF migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Migrations do Domínio
        var context = services.GetRequiredService<NexasDbContext>();
        context.Database.Migrate();

        // Migrations do Identity
        var identityContext = services.GetRequiredService<Nexas.SystemManager.Infrastructure.SystemManagerDbContext>();
        identityContext.Database.Migrate();

        // Popular (Seeder) as Roles no banco de dados
        Nexas.SystemManager.Infrastructure.DataSeeder.SeedRolesAsync(services).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database or seeding roles.");
    }
}

app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseCors("DevelopmentCors");
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityErrorNormalization();
app.MapControllers();

var authGroup = app.MapGroup("/api/v1/auth").WithTags("Authenticator");
authGroup.MapCustomRegister();
authGroup.MapIdentityApi<NexasUser>();
app.MapHub<PaymentHub>("/hubs/payment");

app.Run();
