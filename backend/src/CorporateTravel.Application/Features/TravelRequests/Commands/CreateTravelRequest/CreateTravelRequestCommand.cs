using CorporateTravel.Application.Dtos;
using MediatR;
using System;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.CreateTravelRequest;

public class CreateTravelRequestCommand : IRequest<TravelRequestDto>
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; } // This will be set from the authenticated user in the handler
} 