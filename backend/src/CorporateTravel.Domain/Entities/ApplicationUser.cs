using Microsoft.AspNetCore.Identity;

namespace CorporateTravel.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AvatarUrl { get; set; } // URL da foto do usuário
} 