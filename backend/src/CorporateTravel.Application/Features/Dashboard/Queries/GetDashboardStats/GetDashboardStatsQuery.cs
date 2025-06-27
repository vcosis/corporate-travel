using CorporateTravel.Application.Dtos;
using MediatR;
using System.Collections.Generic;

namespace CorporateTravel.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
    public string? UserId { get; set; }
    public List<string> UserRoles { get; set; } = new List<string>();
} 