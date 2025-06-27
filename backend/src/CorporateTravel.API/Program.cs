using CorporateTravel.Application;
using CorporateTravel.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Debug: Log all arguments
    Console.WriteLine($"Arguments received: [{string.Join(", ", args)}]");

    // Verifica se o parâmetro --seed ou -s foi passado
    var seedDatabase = args.Contains("--seed") || args.Contains("-s");
    Console.WriteLine($"Seed database flag: {seedDatabase}");

    // Add services to the container.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp", policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed(origin => true); // Para debug
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
                Console.WriteLine($"=== JWT OnMessageReceived ===");
                Console.WriteLine($"Path: {path}");
                Console.WriteLine($"Access token exists: {!string.IsNullOrEmpty(accessToken)}");
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationhub"))
                {
                    context.Token = accessToken;
                    Console.WriteLine($"Token set for SignalR connection");
                }
                Console.WriteLine($"=== End JWT OnMessageReceived ===");
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
                Console.WriteLine($"=== JWT Token Validated ===");
                Console.WriteLine($"User ID: {context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
                Console.WriteLine($"User Email: {context.Principal?.FindFirst(ClaimTypes.Email)?.Value}");
                Console.WriteLine($"User Name: {context.Principal?.FindFirst(ClaimTypes.Name)?.Value}");
                var roles = context.Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                Console.WriteLine($"User Roles: [{string.Join(", ", roles ?? new List<string>())}]");
                Console.WriteLine($"=== End JWT Token Validated ===");
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

    Console.WriteLine("SignalR services added to DI container");

    builder.Services.AddControllers();

    var app = builder.Build();

    if (seedDatabase)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                Console.WriteLine("Rodando seed do banco de dados...");
                await CorporateTravel.Infrastructure.Data.DataSeeder.SeedRolesAndAdminAsync(services);
                Console.WriteLine("Seed concluído com sucesso.");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Erro ao rodar o seed.");
                Console.WriteLine($"Erro ao rodar o seed: {ex}");
            }
        }

    }

    // Pipeline normal da aplicação
    app.UseCors("AllowAngularApp");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<CorporateTravel.Infrastructure.Hubs.NotificationHub>("/notificationhub");

    Console.WriteLine("Aplicação iniciada normalmente!");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Exceção global capturada: {ex}");
    throw;
}

// O record deve ficar fora do try/catch
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
