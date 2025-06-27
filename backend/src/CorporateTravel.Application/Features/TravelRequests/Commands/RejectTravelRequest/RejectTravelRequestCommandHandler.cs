using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.RejectTravelRequest;

public class RejectTravelRequestCommandHandler : IRequestHandler<RejectTravelRequestCommand, CommandResult>
{
    private readonly ITravelRequestRepository _repository;

    public RejectTravelRequestCommandHandler(ITravelRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult> Handle(RejectTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travelRequest = await _repository.GetByIdAsync(request.Id);
        if (travelRequest == null)
        {
            return CommandResult.Failure("Travel request not found");
        }

        if (travelRequest.Status != TravelRequestStatus.Pending)
        {
            return CommandResult.Failure("Only pending travel requests can be rejected");
        }

        travelRequest.Status = TravelRequestStatus.Rejected;
        travelRequest.ApproverId = request.ApproverId;
        travelRequest.UpdatedAt = DateTime.UtcNow;
        // We don't set ApprovalDate for rejections

        await _repository.UpdateAsync(travelRequest);

        return CommandResult.Success("Travel request rejected successfully");
    }
} 