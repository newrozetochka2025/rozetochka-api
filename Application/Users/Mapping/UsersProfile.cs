using AutoMapper;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Users.Mapping
{
    public class UsersProfile : Profile
    {
        public UsersProfile()
        {
            // Request DTO -> Entity
            CreateMap<UserRegisterRequestDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())            // Id генерируется БД
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // Хэш задаём в сервисе
             
            
            // Entity -> Response DTO
            CreateMap<User, UserResponseDto>()
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => new DateTimeOffset(src.CreatedAt).ToUnixTimeSeconds()));


        }
    }
}
