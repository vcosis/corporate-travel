using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace CorporateTravel.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"=== NotificationHub.OnConnectedAsync ===");
        Console.WriteLine($"ConnectionId: {Context.ConnectionId}");
        
        var user = Context.User;
        Console.WriteLine($"User authenticated: {user?.Identity?.IsAuthenticated}");
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"User roles: [{string.Join(", ", roles)}]");
            
            if (!string.IsNullOrEmpty(userId))
            {
                // Verificar se já está no grupo antes de adicionar
                var userGroup = $"user_{userId}";
                Console.WriteLine($"Adding to user group: {userGroup}");
                await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
                Console.WriteLine($"Added to group: {userGroup}");
                
                // Adicionar ao grupo de gestores se o usuário for Manager ou Admin
                if (roles.Any(r => r == "Manager" || r == "Admin"))
                {
                    Console.WriteLine($"User has manager/admin role, adding to managers group");
                    await Groups.AddToGroupAsync(Context.ConnectionId, "managers");
                    Console.WriteLine($"Added to group: managers");
                }
                else
                {
                    Console.WriteLine($"User does not have manager/admin role, skipping managers group");
                }
            }
        }
        
        Console.WriteLine($"=== End NotificationHub.OnConnectedAsync ===");
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
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
} 