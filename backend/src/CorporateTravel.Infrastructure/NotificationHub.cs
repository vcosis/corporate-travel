using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Serilog;

namespace CorporateTravel.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger _logger;

    public NotificationHub()
    {
        _logger = Log.ForContext<NotificationHub>();
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var user = Context.User;
        
        _logger.Information("SignalR connection established - ConnectionId: {ConnectionId}, User authenticated: {IsAuthenticated}", 
            connectionId, user?.Identity?.IsAuthenticated);
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            _logger.Debug("Authenticated user - UserId: {UserId}, Roles: {Roles}", userId, roles);
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Verificar se já está no grupo antes de adicionar
                var userGroup = $"user_{userId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
                _logger.Debug("User added to group: {UserGroup}", userGroup);
                
                // Adicionar ao grupo de gestores se o usuário for Manager ou Admin
                if (roles.Any(r => r == "Manager" || r == "Admin"))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
                    _logger.Debug("User added to managers group");
                }
            }
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "managers");
                _logger.Debug("User removed from groups - UserId: {UserId}", userId);
            }
        }
        
        if (exception != null)
        {
            _logger.Warning(exception, "SignalR connection disconnected with exception");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 