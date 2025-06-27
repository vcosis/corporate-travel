using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using MediatR;

namespace CorporateTravel.Application.Features.TravelRequests.Queries.GetTravelRequestById;

public class GetTravelRequestByIdQueryHandler : IRequestHandler<GetTravelRequestByIdQuery, TravelRequestDto?>
{
    private readonly ITravelRequestRepository _repository;
    private readonly IMapper _mapper;

    public GetTravelRequestByIdQueryHandler(ITravelRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TravelRequestDto?> Handle(GetTravelRequestByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var travelRequest = await _repository.GetByIdAsync(request.Id);
            
            if (travelRequest == null)
                return null;
            
            return _mapper.Map<TravelRequestDto>(travelRequest);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving travel request with ID {request.Id}: {ex.Message}", ex);
        }
    }
} 