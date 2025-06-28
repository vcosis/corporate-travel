using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.DeleteTravelRequest;

public class DeleteTravelRequestCommandHandler : IRequestHandler<DeleteTravelRequestCommand, CommandResult>
{
    private readonly ITravelRequestRepository _repository;

    public DeleteTravelRequestCommandHandler(ITravelRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult> Handle(DeleteTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travelRequest = await _repository.GetByIdAsync(request.Id);
        if (travelRequest == null)
        {
            return CommandResult.Failure("Travel request not found");
        }

        // Only allow deletion for pending requests
        if (travelRequest.Status != TravelRequestStatus.Pending)
        {
            return CommandResult.Failure("Only pending travel requests can be deleted");
        }

        // Only administrators can delete travel requests
        if (!request.UserRoles.Contains("Admin"))
        {
            return CommandResult.Failure("Only administrators can delete travel requests");
        }

        await _repository.DeleteAsync(request.Id);

        return CommandResult.Success("Travel request deleted successfully");
    }
} 