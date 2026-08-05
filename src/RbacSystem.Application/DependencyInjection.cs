using Microsoft.Extensions.DependencyInjection;

namespace RbacSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application-layer services here as they are created
        return services;
    }
}
