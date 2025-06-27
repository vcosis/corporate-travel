using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Domain.Entities;

namespace CorporateTravel.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TravelRequest, TravelRequestDto>()
            .ForMember(dest => dest.RequestCode, opt => opt.MapFrom(src => src.RequestCode))
            .ForMember(dest => dest.RequestingUserName, opt => opt.MapFrom(src => src.RequestingUser.Name))
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.Name : null))
            .ForMember(dest => dest.ApprovalDate, opt => opt.MapFrom(src => src.ApprovalDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString().ToLower()));
        
        CreateMap<Notification, NotificationDto>();
        
        // Add other mappings here
    }
} 