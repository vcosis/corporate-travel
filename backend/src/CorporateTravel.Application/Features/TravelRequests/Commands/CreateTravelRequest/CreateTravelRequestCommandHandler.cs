using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.CreateTravelRequest;

public class CreateTravelRequestCommandHandler : IRequestHandler<CreateTravelRequestCommand, TravelRequestDto>
{
    private readonly ITravelRequestRepository _repository;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IRequestCodeService _requestCodeService;

    public CreateTravelRequestCommandHandler(
        ITravelRequestRepository repository, 
        IMapper mapper,
        INotificationService notificationService,
        IRequestCodeService requestCodeService)
    {
        _repository = repository;
        _mapper = mapper;
        _notificationService = notificationService;
        _requestCodeService = requestCodeService;
    }

    public async Task<TravelRequestDto> Handle(CreateTravelRequestCommand request, CancellationToken cancellationToken)
    {
        var travelRequest = new TravelRequest
        {
            RequestCode = await _requestCodeService.GenerateRequestCodeAsync(),
            Origin = request.Origin,
            Destination = request.Destination,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            RequestingUserId = request.RequestingUserId,
            Status = Domain.Enums.TravelRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdRequest = await _repository.AddAsync(travelRequest);

        // Enviar notificação para todos os gestores
        await _notificationService.SendNotificationToManagersAsync(
            title: "Nova Requisição de Viagem",
            message: $"Uma nova requisição de viagem foi criada. Código: {createdRequest.RequestCode}. Origem: {request.Origin}, Destino: {request.Destination}",
            type: NotificationType.Info,
            relatedEntityId: createdRequest.Id.ToString(),
            relatedEntityType: "TravelRequest"
        );

        return _mapper.Map<TravelRequestDto>(createdRequest);
    }
} 