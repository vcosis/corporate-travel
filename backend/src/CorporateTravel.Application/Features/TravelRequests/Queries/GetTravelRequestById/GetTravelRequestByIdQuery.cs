using CorporateTravel.Application.Dtos;
using MediatR;

namespace CorporateTravel.Application.Features.TravelRequests.Queries.GetTravelRequestById;

public class GetTravelRequestByIdQuery : IRequest<TravelRequestDto?>
{
    public Guid Id { get; set; }
} 