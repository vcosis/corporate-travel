using CorporateTravel.Application.Dtos;
using MediatR;
using System;
using System.Collections.Generic;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.DeleteTravelRequest;

public class DeleteTravelRequestCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
    public List<string> UserRoles { get; set; } = new();
} 