using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;

public class GetAllTravelRequestsQueryHandler : IRequestHandler<GetAllTravelRequestsQuery, PaginatedResult<TravelRequestDto>>
{
    private readonly ITravelRequestRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public GetAllTravelRequestsQueryHandler(ITravelRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = Log.ForContext<GetAllTravelRequestsQueryHandler>();
    }

    public async Task<PaginatedResult<TravelRequestDto>> Handle(GetAllTravelRequestsQuery request, CancellationToken cancellationToken)
    {
        // Se o usuário tem role de Manager ou Admin, mostra todas as requisições
        // Se tem role de User, mostra apenas suas próprias requisições
        var isManagerOrAdmin = request.UserRoles.Any(r => r == "Manager" || r == "Admin");
        
        _logger.Debug("GetAllTravelRequests - UserId: {UserId}, UserRoles: {UserRoles}, IsManagerOrAdmin: {IsManagerOrAdmin}, Filtering by userId: {FilteringByUserId}, Additional filters: Period={Period}, SortBy={SortBy}, SortOrder={SortOrder}", 
            request.UserId, request.UserRoles, isManagerOrAdmin, isManagerOrAdmin ? "null (all requests)" : request.UserId, request.Period, request.SortBy, request.SortOrder);
        
        var result = await _repository.GetPaginatedAsync(
            page: request.Page,
            pageSize: request.PageSize,
            status: request.Status,
            searchTerm: request.SearchTerm,
            userId: isManagerOrAdmin ? null : request.UserId,
            period: request.Period,
            requestingUser: request.RequestingUser,
            approver: request.Approver,
            sortBy: request.SortBy,
            sortOrder: request.SortOrder,
            startDate: request.StartDate,
            endDate: request.EndDate
        );

        _logger.Debug("Repository returned {ItemCount} items out of {TotalCount} total", result.Items.Count(), result.TotalCount);

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