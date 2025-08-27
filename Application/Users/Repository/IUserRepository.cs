using Microsoft.EntityFrameworkCore.Storage;
using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Users.Repository
{
    public interface IUserRepository
    {
        Task<bool> IsEmailTakenAsync(string email);
        Task<bool> IsUsernameTakenAsync(string username);
        Task AddUserAsync(User user);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);

    }
}
