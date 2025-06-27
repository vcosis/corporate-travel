using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using CorporateTravel.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CorporateTravel.Infrastructure.Hubs;

namespace CorporateTravel.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationHub _notificationHub;
    private readonly IMapper _mapper;

    public NotificationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        INotificationHub notificationHub,
        IMapper mapper)
    {
        _context = context;
        _userManager = userManager;
        _notificationHub = notificationHub;
        _mapper = mapper;
    }

    public async Task<NotificationDto> CreateNotificationAsync(string title, string message, NotificationType type, Guid recipientId, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title,
            Message = message,
            Type = type.ToString().ToLower(),
            RecipientId = recipientId,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        var notificationDto = _mapper.Map<NotificationDto>(notification);

        // Preencher RequesterName e RequesterAvatarUrl se for TravelRequest
        if (relatedEntityType == "TravelRequest" && relatedEntityId != null)
        {
            if (Guid.TryParse(relatedEntityId, out var travelRequestId))
            {
                var travelRequest = await _context.TravelRequests
                    .Include(tr => tr.RequestingUser)
                    .FirstOrDefaultAsync(tr => tr.Id == travelRequestId);
                if (travelRequest != null && travelRequest.RequestingUser != null)
                {
                    notificationDto.RequesterName = travelRequest.RequestingUser.Name;
                    notificationDto.RequesterAvatarUrl = travelRequest.RequestingUser.AvatarUrl;
                }
            }
        }

        // Enviar notificação em tempo real via SignalR
        await _notificationHub.SendNotificationToUserAsync(recipientId.ToString(), notificationDto);

        return notificationDto;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, bool includeRead = false)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientId == userId);

        if (!includeRead)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
        var notificationDtos = _mapper.Map<List<NotificationDto>>(notifications);

        // Preencher RequesterName e RequesterAvatarUrl para TravelRequest
        foreach (var dto in notificationDtos)
        {
            if (dto.RelatedEntityType == "TravelRequest" && !string.IsNullOrEmpty(dto.RelatedEntityId))
            {
                if (Guid.TryParse(dto.RelatedEntityId, out var travelRequestId))
                {
                    var travelRequest = await _context.TravelRequests
                        .Include(tr => tr.RequestingUser)
                        .FirstOrDefaultAsync(tr => tr.Id == travelRequestId);
                    if (travelRequest != null && travelRequest.RequestingUser != null)
                    {
                        dto.RequesterName = travelRequest.RequestingUser.Name;
                        dto.RequesterAvatarUrl = travelRequest.RequestingUser.AvatarUrl;
                    }
                }
            }
        }

        return notificationDtos;
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Atualizar contador de notificações não lidas
            var unreadCount = await GetUnreadCountAsync(userId);
            await _notificationHub.UpdateUnreadCountAsync(userId.ToString(), unreadCount);
        }
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Atualizar contador de notificações não lidas
        await _notificationHub.UpdateUnreadCountAsync(userId.ToString(), 0);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);
    }

    public async Task SendNotificationToManagersAsync(string title, string message, NotificationType type, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        Console.WriteLine($"=== NotificationService.SendNotificationToManagersAsync ===");
        Console.WriteLine($"Title: {title}");
        Console.WriteLine($"Message: {message}");
        Console.WriteLine($"Type: {type}");
        Console.WriteLine($"RelatedEntityId: {relatedEntityId}");
        Console.WriteLine($"RelatedEntityType: {relatedEntityType}");
        
        // Buscar todos os usuários com role Manager ou Admin
        var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
        var allManagers = managerUsers.Union(adminUsers).Distinct().ToList();

        Console.WriteLine($"Manager users found: {managerUsers.Count}");
        Console.WriteLine($"Admin users found: {adminUsers.Count}");
        Console.WriteLine($"Total unique managers: {allManagers.Count}");
        
        // Log detalhado de cada manager
        foreach (var manager in allManagers)
        {
            Console.WriteLine($"Manager: {manager.UserName} (ID: {manager.Id})");
        }

        foreach (var manager in allManagers)
        {
            Console.WriteLine($"Creating notification for manager: {manager.UserName} (ID: {manager.Id})");
            var notification = await CreateNotificationAsync(title, message, type, manager.Id, relatedEntityId, relatedEntityType);
            Console.WriteLine($"Notification created with ID: {notification.Id}");
        }

        Console.WriteLine($"=== End NotificationService.SendNotificationToManagersAsync ===");
    }
} 