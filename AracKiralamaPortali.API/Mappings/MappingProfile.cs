using AutoMapper;
using AracKiralamaPortali.API.Models;
using AracKiralamaPortali.API.DTOs;

namespace AracKiralamaPortali.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src => src.Reviews.Count > 0 ? src.Reviews.Average(r => r.Rating) : 0))
                .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList()))
                .ForMember(dest => dest.OwnerId, opt => opt.MapFrom(src => src.OwnerId));

            CreateMap<VehicleCreateDto, Vehicle>()
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore()); // Will map this manually or later
                
            CreateMap<VehicleUpdateDto, Vehicle>();
        }
    }
}
