using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Application.Users.DTOs
{
    public class UserLoginRequestDto
    {
        private const int EmailMaxLength = 100;
        private const int PasswordMaxLength = 100;

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} is required")]
        [EmailAddress(ErrorMessage = "Invalid {0} format")]
        [StringLength(EmailMaxLength, ErrorMessage = "{0} must be {1} characters")]
        public string Email { get; set; } = "";

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(PasswordMaxLength, ErrorMessage = "{0} must be {1} characters")]
        public string Password { get; set; } = "";
    }
}
