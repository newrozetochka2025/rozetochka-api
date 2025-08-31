using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Application.Users.DTOs
{
    public class RefreshRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = "";
    }
}
