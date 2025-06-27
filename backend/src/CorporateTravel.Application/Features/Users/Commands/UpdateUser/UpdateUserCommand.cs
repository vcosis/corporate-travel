using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CorporateTravel.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<IdentityResult>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
} 