namespace rozetochka_api.Application.Users.DTOs
{

    // отдаём при логине/рефреше: пара токенов + данные пользователя
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = "";   // JWT для Authorization: Bearer
        public string RefreshToken { get; set; } = "";  // одноразовый refresh
        public int ExpiresIn { get; set; }              // сколько секунд живет access
        public UserResponseDto User { get; set; } = null!;
    }
}
