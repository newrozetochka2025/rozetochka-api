using Microsoft.EntityFrameworkCore.Storage;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Users.Repository
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);

        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);

        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> IsUsernameTakenAsync(string username);

        // Refresh tokens
        Task AddRefreshTokenAsync(UserRefreshToken token);
        Task<UserRefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(UserRefreshToken token);

    }
}
