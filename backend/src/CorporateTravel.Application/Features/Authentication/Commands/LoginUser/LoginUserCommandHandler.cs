using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.Authentication.Commands.LoginUser;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IPasswordRequirementsService _passwordRequirementsService;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager, 
        ITokenService tokenService,
        IPasswordRequirementsService passwordRequirementsService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _passwordRequirementsService = passwordRequirementsService;
    }

    public async Task<AuthResponseDto?> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        if (user == null)
        {
            return null;
        }

        // Verificar se a senha está no formato correto antes de tentar autenticar
        if (!_passwordRequirementsService.ValidatePassword(request.Password, out var passwordErrors))
        {
            // Retornar null para indicar falha na autenticação, mas o frontend pode usar as validações
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return null;
        }

        var tokenString = await _tokenService.GenerateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = tokenString,
            Name = user.Name,
            Email = user.Email!,
            Roles = roles.ToList(),
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
    }
} 