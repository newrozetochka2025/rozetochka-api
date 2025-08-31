using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Data;

namespace rozetochka_api.Application.Users.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddUserAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _db.Users.AnyAsync(u => u.Username == username);
        }

        // Refresh токены
        public async Task AddRefreshTokenAsync(UserRefreshToken token)
        {
            await _db.UserRefreshTokens.AddAsync(token);
            await _db.SaveChangesAsync();
        }

        public async Task<UserRefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _db.UserRefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == token && !t.IsRevoked);
        }
            

        public async Task RevokeRefreshTokenAsync(UserRefreshToken token)
        {
            token.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }
}
