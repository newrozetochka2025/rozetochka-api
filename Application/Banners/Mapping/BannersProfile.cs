using AutoMapper;
using rozetochka_api.Application.Banners.DTOs;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Banners.Mapping
{
    public class BannersProfile : Profile
    {
        public BannersProfile()
        {
            // ENtity -> DTO
            CreateMap<Banner, BannerResponseDto>()
                .ForMember(d => d.Img, m => m.MapFrom(s => s.ImgUrl));
        }
    }
}
