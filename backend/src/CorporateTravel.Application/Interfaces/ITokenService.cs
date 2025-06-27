using System.Security.Claims;
using CorporateTravel.Domain.Entities;

namespace CorporateTravel.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    ClaimsPrincipal? ValidateToken(string token);
} 