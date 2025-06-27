using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CorporateTravel.Application.Features.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly ITravelRequestRepository _repository;

    public GetDashboardStatsQueryHandler(ITravelRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Se o usuário tem role de Manager ou Admin, mostra todas as requisições
        // Se tem role de User, mostra apenas suas próprias requisições
        var isManagerOrAdmin = request.UserRoles.Any(r => r == "Manager" || r == "Admin");
        
        var statusCounts = await _repository.GetStatusCountsAsync(
            userId: isManagerOrAdmin ? null : request.UserId
        );
        
        return new DashboardStatsDto 
        { 
            StatusCounts = statusCounts,
            Pending = statusCounts.GetValueOrDefault("Pending", 0),
            Approved = statusCounts.GetValueOrDefault("Approved", 0),
            Rejected = statusCounts.GetValueOrDefault("Rejected", 0),
            Total = statusCounts.Values.Sum()
        };
    }
} 