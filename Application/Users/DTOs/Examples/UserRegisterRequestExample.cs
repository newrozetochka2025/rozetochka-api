using Swashbuckle.AspNetCore.Filters;

namespace rozetochka_api.Application.Users.DTOs.Examples
{
    public class UserRegisterRequestExample : IExamplesProvider<UserRegisterRequestDto>
    {
        public UserRegisterRequestDto GetExamples() => new()
        {
            Email = "user@example.com",
            Username = "tester",
            Password = "P@ssw0rd",
            PasswordRepeat = "P@ssw0rd",
            Phone = "+380967775533"
        };
    }
}
