using rozetochka_api.Infrastructure.Identity.Interfaces;

namespace rozetochka_api.Infrastructure.Identity
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 10;

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
