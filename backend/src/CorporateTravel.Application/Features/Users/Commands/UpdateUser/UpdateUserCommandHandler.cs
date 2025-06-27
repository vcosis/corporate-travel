using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, IdentityResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        }

        // Verificar se o email já existe em outro usuário
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Email já está em uso por outro usuário" });
        }

        // Atualizar propriedades do usuário
        user.Name = request.Name;
        user.Email = request.Email;
        user.UserName = request.Email;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return updateResult;
        }

        // Atualizar role do usuário
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return IdentityResult.Success;
    }
} 