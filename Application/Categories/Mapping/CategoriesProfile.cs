using AutoMapper;
using rozetochka_api.Application.Categories.DTOs;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Categories.Mapping
{
    public class CategoriesProfile : Profile
    {
        public CategoriesProfile()
        {
            // Category -> DTO
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(d => d.Svg, m => m.MapFrom(s => s.IconSvgUrl))
                .ForMember(d => d.Href, m => m.MapFrom(s => s.Slug));
        }
    }
}
