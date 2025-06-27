using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using CorporateTravel.Infrastructure.Hubs;

namespace CorporateTravel.Infrastructure.Services;

public class NotificationHubService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(string userId, NotificationDto notification)
    {
        Console.WriteLine($"=== NotificationHubService.SendNotificationToUserAsync ===");
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Notification: {notification?.Title} - {notification?.Message}");
        Console.WriteLine($"Notification ID: {notification?.Id}");
        
        // Validar se a notificação não é nula ou vazia
        if (notification == null)
        {
            Console.WriteLine("ERROR: Notification is null, skipping send");
            Console.WriteLine($"=== End NotificationHubService.SendNotificationToUserAsync (null notification) ===");
            return;
        }
        
        if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Message))
        {
            Console.WriteLine("ERROR: Notification has empty title or message, skipping send");
            Console.WriteLine($"=== End NotificationHubService.SendNotificationToUserAsync (empty notification) ===");
            return;
        }
        
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        Console.WriteLine($"Notification sent to group: user_{userId}");
        Console.WriteLine($"=== End NotificationHubService.SendNotificationToUserAsync ===");
    }

    public async Task SendNotificationToGroupAsync(string groupName, object notification)
    {
        Console.WriteLine($"=== NotificationHubService.SendNotificationToGroupAsync ===");
        Console.WriteLine($"Group: {groupName}");
        Console.WriteLine($"Notification: {notification}");
        
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        Console.WriteLine($"Notification sent to group: {groupName}");
        Console.WriteLine($"=== End NotificationHubService.SendNotificationToGroupAsync ===");
    }

    public async Task UpdateUnreadCountAsync(string userId, int count)
    {
        Console.WriteLine($"=== NotificationHubService.UpdateUnreadCountAsync ===");
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Count: {count}");
        
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateUnreadCount", count);
        Console.WriteLine($"Unread count updated for group: user_{userId}");
        Console.WriteLine($"=== End NotificationHubService.UpdateUnreadCountAsync ===");
    }
} 