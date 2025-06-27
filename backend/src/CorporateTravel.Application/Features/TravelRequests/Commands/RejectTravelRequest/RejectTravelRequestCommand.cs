using CorporateTravel.Application.Dtos;
using MediatR;
using System;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.RejectTravelRequest;

public class RejectTravelRequestCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public Guid ApproverId { get; set; }
} 