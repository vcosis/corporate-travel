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
    private readonly INotificationService _notificationService;

    public RejectTravelRequestCommandHandler(ITravelRequestRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
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

        // Criar notificação para o usuário que criou a requisição
        await _notificationService.CreateNotificationAsync(
            "Requisição de Viagem Rejeitada",
            $"Sua requisição de viagem para {travelRequest.Destination} foi rejeitada.",
            NotificationType.Warning,
            travelRequest.RequestingUserId,
            travelRequest.Id.ToString(),
            "TravelRequest"
        );

        return CommandResult.Success("Travel request rejected successfully");
    }
} 