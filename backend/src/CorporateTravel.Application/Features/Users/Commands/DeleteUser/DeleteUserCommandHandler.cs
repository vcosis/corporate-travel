using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using System;

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
        Console.WriteLine($"=== DeleteUserCommandHandler ===");
        Console.WriteLine($"Request ID: '{request.Id}'");
        Console.WriteLine($"Request ID type: {request.Id?.GetType()}");
        Console.WriteLine($"Request ID length: {request.Id?.Length}");
        
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            Console.WriteLine($"User found: {user != null}");
            
            if (user == null)
            {
                Console.WriteLine("User not found");
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
            }

            Console.WriteLine($"Found user: {user.UserName} ({user.Email})");

            // Verificar se o usuário tem role de Admin (não permitir deletar admins)
            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"User roles: {string.Join(", ", roles)}");
            
            if (roles.Contains("Admin"))
            {
                Console.WriteLine("Cannot delete admin user");
                return IdentityResult.Failed(new IdentityError { Description = "Não é possível deletar um usuário administrador" });
            }

            var result = await _userManager.DeleteAsync(user);
            Console.WriteLine($"Delete result: {result.Succeeded}");
            if (!result.Succeeded)
            {
                Console.WriteLine($"Delete errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in DeleteUserCommandHandler: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return IdentityResult.Failed(new IdentityError { Description = $"Erro interno: {ex.Message}" });
        }
    }
} 