using MediatR;
using CorporateTravel.Application.Dtos;

namespace CorporateTravel.Application.Features.Profile.Queries.GetProfile;

public class GetProfileQuery : IRequest<ProfileDto>
{
    public string UserId { get; set; } = string.Empty;
} 