using CorporateTravel.Application.Dtos;

namespace CorporateTravel.Application.Interfaces;

public interface INotificationHub
{
    Task SendNotificationToUserAsync(string userId, NotificationDto notification);
    Task SendNotificationToGroupAsync(string groupName, object notification);
    Task UpdateUnreadCountAsync(string userId, int count);
} 