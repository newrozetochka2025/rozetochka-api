using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Users.Service
{
    public interface IUserService
    {
        Task<ServiceResult<UserResponseDto>> RegisterUserAsync(UserRegisterRequestDto dto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(UserLoginRequestDto dto);
        Task<ServiceResult<AuthResponseDto>> RefreshAsync(string refreshToken);



    }
}
