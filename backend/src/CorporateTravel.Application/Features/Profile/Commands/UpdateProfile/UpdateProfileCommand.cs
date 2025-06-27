using MediatR;
using CorporateTravel.Application.Dtos;

namespace CorporateTravel.Application.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommand : IRequest<CommandResult>
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
} 