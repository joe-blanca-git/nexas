using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Nexas.Application.Common.Behaviors;
using Nexas.Application.Common.Interfaces;
using Nexas.Application.Common.Services;
using System.Reflection;

namespace Nexas.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<IUserContextService, UserContextService>();

            return services;
        }
    }
}
