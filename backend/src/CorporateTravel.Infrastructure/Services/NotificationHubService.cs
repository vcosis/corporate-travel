using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using CorporateTravel.Infrastructure.Hubs;
using Serilog;

namespace CorporateTravel.Infrastructure.Services;

public class NotificationHubService : INotificationHub
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger _logger;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
        _logger = Log.ForContext<NotificationHubService>();
    }

    public async Task SendNotificationToUserAsync(string userId, NotificationDto notification)
    {
        _logger.Debug("Sending notification to user - UserId: {UserId}, Notification: {Title} - {Message}, ID: {NotificationId}", 
            userId, notification?.Title, notification?.Message, notification?.Id);
        
        // Validar se a notificação não é nula ou vazia
        if (notification == null)
        {
            _logger.Warning("Notification is null, skipping send for user {UserId}", userId);
            return;
        }
        
        if (string.IsNullOrEmpty(notification.Title) || string.IsNullOrEmpty(notification.Message))
        {
            _logger.Warning("Notification has empty title or message, skipping send for user {UserId}", userId);
            return;
        }
        
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        _logger.Debug("Notification sent to group: user_{UserId}", userId);
    }

    public async Task SendNotificationToGroupAsync(string groupName, object notification)
    {
        _logger.Debug("Sending notification to group - Group: {GroupName}, Notification: {Notification}", 
            groupName, notification);
        
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        _logger.Debug("Notification sent to group: {GroupName}", groupName);
    }

    public async Task UpdateUnreadCountAsync(string userId, int count)
    {
        _logger.Debug("Updating unread count - UserId: {UserId}, Count: {Count}", userId, count);
        
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateUnreadCount", count);
        _logger.Debug("Unread count updated for group: user_{UserId}", userId);
    }
} 