using CorporateTravel.Application.Dtos;
using MediatR;
using System.Collections.Generic;

namespace CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;

public class GetAllTravelRequestsQuery : IRequest<PaginatedResult<TravelRequestDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public string? UserId { get; set; }
    public List<string> UserRoles { get; set; } = new List<string>();
    
    // Novos filtros refinados
    public string? Period { get; set; }
    public string? RequestingUser { get; set; }
    public string? Approver { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
} 