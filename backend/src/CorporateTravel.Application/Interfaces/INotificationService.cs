using CorporateTravel.Application.Dtos;
using CorporateTravel.Domain.Enums;

namespace CorporateTravel.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(string title, string message, NotificationType type, Guid recipientId, string? relatedEntityId = null, string? relatedEntityType = null);
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool includeRead = false);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task SendNotificationToManagersAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null);
} 