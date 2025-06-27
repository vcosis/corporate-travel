using Microsoft.AspNetCore.Identity;

namespace CorporateTravel.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    // Parameterless constructor for EF Core
    public ApplicationRole() : base()
    {
    }
} 