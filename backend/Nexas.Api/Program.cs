using Nexas.Api.Extensions;
using Nexas.Api.Middlewares;
using Nexas.Api.Services;
using Nexas.Application;
using Nexas.Application.Common.Interfaces;
using Nexas.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. Configuração de Serviços (Dependency Injection)
// ============================================================

// Configuração de CORS para desenvolvimento
// Permite que o teu Front-end (JS) aceda à API sem bloqueios
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentCors", policy =>
    {
        policy.AllowAnyOrigin()   // Permite qualquer domínio (ex: localhost, 127.0.0.1)
              .AllowAnyMethod()   // Permite GET, POST, PUT, DELETE, etc.
              .AllowAnyHeader();  // Permite qualquer cabeçalho (ex: Authorization, Content-Type)
    });
});

// Setup das camadas da Clean Architecture
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationSetup(builder.Configuration);

// Serviços de Contexto de Utilizador e Autenticação
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerSetup();

var app = builder.Build();

// ============================================================
// 2. Configuração do Pipeline de Middleware (HTTP Request)
// ============================================================

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // O prefixo /nexas deve bater com o que você colocou no Nginx
        c.SwaggerEndpoint("/nexas/swagger/v1/swagger.json", "Nexas API V1");
        
        // Define a rota onde o Swagger será acessado (joederblanca.com.br/nexas/swagger)
        c.RoutePrefix = "swagger"; 
    });
//}

// O UseCors deve vir antes da Autenticação/Autorização
app.UseCors("DevelopmentCors");

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

// Middleware de Autenticação (valida o Token JWT do Agivys)
app.UseAuthentication();

// Middleware de Autorização (valida Roles e Permissões)
app.UseAuthorization();

app.MapControllers();

app.Run();