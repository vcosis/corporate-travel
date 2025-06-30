using CorporateTravel.Application.Interfaces;
using CorporateTravel.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MediatR;

namespace CorporateTravel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register application services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordRequirementsService, PasswordRequirementsService>();
        
        return services;
    }
} 