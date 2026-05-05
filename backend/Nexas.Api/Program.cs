using Nexas.Api.Extensions;
using Nexas.Api.Middlewares;
using Nexas.Api.Services;
using Nexas.Application;
using Nexas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Dependency Injection setup
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthenticationSetup(builder.Configuration);

builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Middleware pipeline configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Extract user context after auth
app.UseUserContext();

app.MapControllers();

app.Run();
