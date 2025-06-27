using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, IdentityResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IdentityResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
        }

        // Verificar se o usuário tem role de Admin (não permitir deletar admins)
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Não é possível deletar um usuário administrador" });
        }

        var result = await _userManager.DeleteAsync(user);
        return result;
    }
} 