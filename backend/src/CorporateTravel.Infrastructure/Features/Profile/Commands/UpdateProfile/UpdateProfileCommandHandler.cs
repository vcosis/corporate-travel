using MediatR;
using CorporateTravel.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Application.Features.Profile.Commands.UpdateProfile;
using CorporateTravel.Infrastructure.Data;

namespace CorporateTravel.Infrastructure.Features.Profile.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, CommandResult>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateProfileCommandHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<CommandResult> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == request.UserId, cancellationToken);

        if (user == null)
        {
            return CommandResult.Failure("Usuário não encontrado");
        }

        // Atualizar informações básicas
        user.Name = request.Name;

        // Se uma nova senha foi fornecida, validar a senha atual
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return CommandResult.Failure("Senha atual é obrigatória para alterar a senha");
            }

            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                return CommandResult.Failure("Senha atual incorreta");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                var errors = string.Join(", ", changePasswordResult.Errors.Select(e => e.Description));
                return CommandResult.Failure($"Erro ao alterar senha: {errors}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return CommandResult.Success("Perfil atualizado com sucesso");
    }
} 