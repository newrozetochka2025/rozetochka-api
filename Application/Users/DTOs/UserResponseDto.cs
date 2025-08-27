namespace rozetochka_api.Application.Users.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Role { get; set; } = "";
        public long CreatedAt { get; set; }
    }
}
