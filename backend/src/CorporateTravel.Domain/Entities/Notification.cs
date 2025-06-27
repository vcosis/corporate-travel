using CorporateTravel.Domain.Enums;

namespace CorporateTravel.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "info", "success", "warning", "error"
    public Guid RecipientId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityId { get; set; } // ID da entidade relacionada (ex: TravelRequest ID)
    public string? RelatedEntityType { get; set; } // Tipo da entidade relacionada (ex: "TravelRequest")

    // Navigation property
    public virtual ApplicationUser Recipient { get; set; } = null!;
} 