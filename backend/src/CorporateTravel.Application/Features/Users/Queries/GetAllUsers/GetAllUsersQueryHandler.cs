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

        // Aplicar filtro por perfil se fornecido
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
        {
            // Filtrar usuários que têm o perfil especificado
            var usersWithRole = await _userManager.GetUsersInRoleAsync(request.RoleFilter);
            var userIdsWithRole = usersWithRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIdsWithRole.Contains(u.Id));
        }

        // Aplicar filtro por status se fornecido
        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            // Por enquanto, vamos considerar todos os usuários como ativos
            // Você pode implementar lógica específica baseada em suas necessidades
            // Por exemplo, baseado em uma propriedade IsActive ou similar
            if (request.StatusFilter.ToLower() == "inactive")
            {
                // Se você tiver uma propriedade IsActive, use: query = query.Where(u => !u.IsActive);
                // Por enquanto, vamos retornar lista vazia para inativo
                query = query.Where(u => false);
            }
        }

        // Aplicar ordenação se fornecida
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            switch (request.SortBy.ToLower())
            {
                case "name":
                    query = query.OrderBy(u => u.Name);
                    break;
                case "email":
                    query = query.OrderBy(u => u.Email);
                    break;
                case "createdat":
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
                default:
                    query = query.OrderBy(u => u.Name);
                    break;
            }
        }
        else
        {
            // Ordenação padrão por nome
            query = query.OrderBy(u => u.Name);
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