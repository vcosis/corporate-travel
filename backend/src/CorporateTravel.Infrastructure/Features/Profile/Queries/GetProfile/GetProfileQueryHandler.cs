using MediatR;
using CorporateTravel.Application.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Application.Features.Profile.Queries.GetProfile;
using CorporateTravel.Infrastructure.Data;

namespace CorporateTravel.Infrastructure.Features.Profile.Queries.GetProfile;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, ProfileDto>
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetProfileQueryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ProfileDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var profileDto = new ProfileDto
        {
            Id = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email!,
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt
        };

        return profileDto;
    }
} 