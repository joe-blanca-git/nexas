using Nexas.Api.Extensions;
using Nexas.Api.Middlewares;
using Nexas.Api.Services;
using Nexas.Api.Hubs;
using Nexas.Application;
using Nexas.Application.Common.Interfaces;
using Nexas.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
                new Uri(origin).Host == "localhost" || 
                new Uri(origin).Host.EndsWith("joederblanca.com.br"))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationSetup(builder.Configuration);
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPaymentEventPublisher, PaymentEventPublisher>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, CustomUserIdProvider>();
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

// Removida a verificação IsDevelopment() para garantir que a correção do proxy 
// seja aplicada mesmo rodando como Development no Docker.
app.UseSwagger(c =>
{
    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
    {
        swaggerDoc.Servers = new List<OpenApiServer>
        {
            // Força o Swagger a usar a URL externa real em todas as requisições (Try it out)
            new OpenApiServer { Url = "https://joederblanca.com.br/nexas-api" }
        };
    });
});

app.UseSwaggerUI(c =>
{
    // Caminho relativo para encontrar o JSON do Swagger de forma segura
    c.SwaggerEndpoint("v1/swagger.json", "Nexas API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("DevelopmentCors");
app.UseGlobalExceptionHandler();

// Desativado pois o Nginx já gerencia o HTTPS na VPS (evita avisos no log)
// app.UseHttpsRedirection(); 

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PaymentHub>("/hubs/payment");

app.Run();
