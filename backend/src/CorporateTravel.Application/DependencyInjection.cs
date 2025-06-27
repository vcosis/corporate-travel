using CorporateTravel.Application.Interfaces;
using CorporateTravel.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CorporateTravel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register application services
        services.AddScoped<ITokenService, TokenService>();
        
        return services;
    }
} 