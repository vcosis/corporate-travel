using CorporateTravel.Application.Dtos;
using MediatR;
using System;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.ApproveTravelRequest;

public class ApproveTravelRequestCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public Guid ApproverId { get; set; }
} 