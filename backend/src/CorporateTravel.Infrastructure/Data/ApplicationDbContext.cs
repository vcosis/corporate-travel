using CorporateTravel.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CorporateTravel.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TravelRequest> TravelRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // AvatarUrl é string, não precisa de configuração especial, mas pode adicionar se quiser limitar tamanho:
        builder.Entity<ApplicationUser>()
            .Property(u => u.AvatarUrl)
            .HasMaxLength(512);
        // Fluent API configurations can be added here or in separate IEntityTypeConfiguration classes
    }
} 