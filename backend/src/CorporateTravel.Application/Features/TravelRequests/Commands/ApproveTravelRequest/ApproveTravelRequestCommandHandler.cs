using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.ApproveTravelRequest;

public class ApproveTravelRequestCommandHandler : IRequestHandler<ApproveTravelRequestCommand, CommandResult>
{
    private readonly ITravelRequestRepository _repository;

    public ApproveTravelRequestCommandHandler(ITravelRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult> Handle(ApproveTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travelRequest = await _repository.GetByIdAsync(request.Id);
        if (travelRequest == null)
        {
            return CommandResult.Failure("Travel request not found");
        }

        if (travelRequest.Status != TravelRequestStatus.Pending)
        {
            return CommandResult.Failure("Only pending travel requests can be approved");
        }

        travelRequest.Status = TravelRequestStatus.Approved;
        travelRequest.ApproverId = request.ApproverId;
        travelRequest.ApprovalDate = DateTime.UtcNow;
        travelRequest.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(travelRequest);

        return CommandResult.Success("Travel request approved successfully");
    }
} 