using CorporateTravel.Application.Dtos;
using MediatR;

namespace CorporateTravel.Application.Features.Authentication.Commands.LoginUser;

public class LoginUserCommand : IRequest<AuthResponseDto?>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 