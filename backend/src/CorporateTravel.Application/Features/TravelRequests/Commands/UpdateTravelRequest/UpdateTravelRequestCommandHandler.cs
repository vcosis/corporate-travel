using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.UpdateTravelRequest;

public class UpdateTravelRequestCommandHandler : IRequestHandler<UpdateTravelRequestCommand, TravelRequestDto>
{
    private readonly ITravelRequestRepository _repository;
    private readonly IMapper _mapper;

    public UpdateTravelRequestCommandHandler(ITravelRequestRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TravelRequestDto> Handle(UpdateTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travelRequest = await _repository.GetByIdAsync(request.Id);
        if (travelRequest == null)
        {
            throw new InvalidOperationException("Travel request not found");
        }

        // Only allow updates for pending requests
        if (travelRequest.Status != TravelRequestStatus.Pending)
        {
            throw new InvalidOperationException("Only pending travel requests can be updated");
        }

        // Only the requesting user can update their own travel request
        if (travelRequest.RequestingUserId != request.RequestingUserId)
        {
            throw new InvalidOperationException("You can only update your own travel requests");
        }

        // Update the travel request
        travelRequest.Origin = request.Origin;
        travelRequest.Destination = request.Destination;
        travelRequest.StartDate = request.StartDate;
        travelRequest.EndDate = request.EndDate;
        travelRequest.Reason = request.Reason;
        travelRequest.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(travelRequest);

        return _mapper.Map<TravelRequestDto>(travelRequest);
    }
} 