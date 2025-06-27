using CorporateTravel.Application.Dtos;
using MediatR;
using System;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.DeleteTravelRequest;

public class DeleteTravelRequestCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
} 