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
    private readonly INotificationService _notificationService;

    public ApproveTravelRequestCommandHandler(ITravelRequestRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
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

        // Criar notificação para o usuário que criou a requisição
        await _notificationService.CreateNotificationAsync(
            "Requisição de Viagem Aprovada",
            $"Sua requisição de viagem para {travelRequest.Destination} foi aprovada.",
            NotificationType.Success,
            travelRequest.RequestingUserId,
            travelRequest.Id.ToString(),
            "TravelRequest"
        );

        return CommandResult.Success("Travel request approved successfully");
    }
} 