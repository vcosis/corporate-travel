using MediatR;
using CorporateTravel.Application.Dtos;

namespace CorporateTravel.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQuery : IRequest<PaginatedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? SortBy { get; set; }
} 