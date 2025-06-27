using CorporateTravel.Application.Dtos;
using CorporateTravel.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PaginatedResult<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PaginatedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        // Aplicar filtro de busca se fornecido
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(searchTerm) || 
                u.Email!.ToLower().Contains(searchTerm));
        }

        // Obter contagem total antes da paginação
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação
        var skip = (request.Page - 1) * request.PageSize;
        var users = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email!,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            });
        }

        var result = new PaginatedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        return result;
    }
} 