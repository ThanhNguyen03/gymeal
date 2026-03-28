using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Gymeal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // Pipeline order: CorrelationId → Logging → Validation → Handler → Audit
            // (Behaviours wired here in Sprint 1+)
        });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
