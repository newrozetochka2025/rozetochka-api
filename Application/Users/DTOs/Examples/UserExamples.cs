using Swashbuckle.AspNetCore.Filters;

namespace rozetochka_api.Application.Users.DTOs.Examples
{




    // Login
    public class UserLoginRequestExample : IExamplesProvider<UserLoginRequestDto>
    {
        public UserLoginRequestDto GetExamples() => new()
        {
            Email = "user@example.com",
            Password = "P@ssw0rd"
        };
    }

    // Register
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
