using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;
using System;
using Serilog;

namespace CorporateTravel.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, IdentityResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger _logger;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
        _logger = Log.ForContext<DeleteUserCommandHandler>();
    }

    public async Task<IdentityResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.Debug("DeleteUserCommandHandler - Request ID: {UserId}, Type: {IdType}, Length: {IdLength}", 
            request.Id, request.Id?.GetType(), request.Id?.Length);
        
        try
        {
            // Validar se o ID não é nulo ou vazio
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                _logger.Warning("Invalid user ID provided");
                return IdentityResult.Failed(new IdentityError { Description = "ID do usuário inválido" });
            }
            
            var user = await _userManager.FindByIdAsync(request.Id);
            _logger.Debug("User found: {UserFound}", user != null);
            
            if (user == null)
            {
                _logger.Warning("User not found - ID: {UserId}", request.Id);
                return IdentityResult.Failed(new IdentityError { Description = "Usuário não encontrado" });
            }

            _logger.Debug("Found user: {UserName} ({Email})", user.UserName, user.Email);

            // Verificar se o usuário tem role de Admin (não permitir deletar admins)
            var roles = await _userManager.GetRolesAsync(user);
            _logger.Debug("User roles: {Roles}", roles);
            
            if (roles.Contains("Admin"))
            {
                _logger.Warning("Cannot delete admin user - UserId: {UserId}", request.Id);
                return IdentityResult.Failed(new IdentityError { Description = "Não é possível deletar um usuário administrador" });
            }

            var result = await _userManager.DeleteAsync(user);
            _logger.Debug("Delete result: {Succeeded}", result.Succeeded);
            if (!result.Succeeded)
            {
                _logger.Warning("Delete errors: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception in DeleteUserCommandHandler - UserId: {UserId}", request.Id);
            return IdentityResult.Failed(new IdentityError { Description = $"Erro interno: {ex.Message}" });
        }
    }
} 