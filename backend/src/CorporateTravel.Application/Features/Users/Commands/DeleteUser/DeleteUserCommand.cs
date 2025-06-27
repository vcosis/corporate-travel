using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CorporateTravel.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<IdentityResult>
{
    public string Id { get; set; } = string.Empty;
} 