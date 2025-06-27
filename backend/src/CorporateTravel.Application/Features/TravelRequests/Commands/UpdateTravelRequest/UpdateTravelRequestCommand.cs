using CorporateTravel.Application.Dtos;
using MediatR;
using System;

namespace CorporateTravel.Application.Features.TravelRequests.Commands.UpdateTravelRequest;

public class UpdateTravelRequestCommand : IRequest<TravelRequestDto>
{
    public Guid Id { get; set; }
    public Guid RequestingUserId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
} 