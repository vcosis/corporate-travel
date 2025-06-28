using CorporateTravel.Application;
using CorporateTravel.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Serilog;
using Microsoft.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // Add services to the container.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200", 
                    "http://localhost:4201",
                    "http://frontend:80",           // Frontend container
                    "http://corporatetravel-frontend:80"  // Frontend container name
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // Configure JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
        
        // Configure JWT for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                Log.Debug("JWT OnMessageReceived - Path: {Path}, Access token exists: {HasToken}", 
                    path, !string.IsNullOrEmpty(accessToken));
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
                {
                    context.Token = accessToken;
                    Log.Debug("Token set for SignalR connection");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"error\": \"Unauthorized\"}");
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                var userName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
                var roles = context.Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                
                Log.Debug("JWT Token Validated - User ID: {UserId}, Email: {Email}, Name: {Name}, Roles: {Roles}", 
                    userId, userEmail, userName, roles);
                
                return Task.CompletedTask;
            }
        };
    });

    // Add SignalR
    builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = true;
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    });

    Log.Information("SignalR services added to DI container");

    builder.Services.AddControllers();

    var app = builder.Build();

    // Pipeline normal da aplicação
    app.UseCors("AllowAngularApp");
    app.UseHttpsRedirection();
    
    // Add Serilog request logging middleware
    app.UseSerilogRequestLogging();
    
    // Ensure database is created and migrations are applied
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CorporateTravel.Infrastructure.Data.ApplicationDbContext>();
        context.Database.Migrate();
        
        // Seed initial data
        await CorporateTravel.Infrastructure.Data.DataSeeder.SeedRolesAndAdminAsync(app.Services);
    }
    
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<CorporateTravel.Infrastructure.Hubs.NotificationHub>("/notificationhub");

    Log.Information("Application started successfully!");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

