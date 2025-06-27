using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;

public class GetAllTravelRequestsQueryHandler : IRequestHandler<GetAllTravelRequestsQuery, PaginatedResult<TravelRequestDto>>
{
    private readonly ITravelRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetAllTravelRequestsQueryHandler(ITravelRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<TravelRequestDto>> Handle(GetAllTravelRequestsQuery request, CancellationToken cancellationToken)
    {
        // Se o usuário tem role de Manager ou Admin, mostra todas as requisições
        // Se tem role de User, mostra apenas suas próprias requisições
        var isManagerOrAdmin = request.UserRoles.Any(r => r == "Manager" || r == "Admin");
        
        Console.WriteLine($"=== GetAllTravelRequestsQueryHandler ===");
        Console.WriteLine($"UserId: {request.UserId}");
        Console.WriteLine($"UserRoles: [{string.Join(", ", request.UserRoles)}]");
        Console.WriteLine($"IsManagerOrAdmin: {isManagerOrAdmin}");
        Console.WriteLine($"Filtering by userId: {(isManagerOrAdmin ? "null (all requests)" : request.UserId)}");
        
        var result = await _repository.GetPaginatedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            status: request.Status,
            searchTerm: request.SearchTerm,
            userId: isManagerOrAdmin ? null : request.UserId
        );

        Console.WriteLine($"Repository returned {result.Items.Count()} items out of {result.TotalCount} total");
        Console.WriteLine($"=== End GetAllTravelRequestsQueryHandler ===");

        var travelRequestDtos = _mapper.Map<IEnumerable<TravelRequestDto>>(result.Items);

        return new PaginatedResult<TravelRequestDto>
        {
            Items = travelRequestDtos.ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
} 