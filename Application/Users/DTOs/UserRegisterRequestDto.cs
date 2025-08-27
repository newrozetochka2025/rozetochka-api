using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Application.Users.DTOs
{
    public class UserRegisterRequestDto
    {
        private const int MinLength = 5;
        private const int MaxLength = 50;
        private const int EmailMaxLength = 100;
        private const int PasswordMinLength = 6;    // TODO later 8

        [Display(Name = "Email")]
        [Required(ErrorMessage = "{0} is required")]  // {0} в ErrorMessage — плейсхолдер для имени поля из [Display] (выше) или имени свойства, упрощает локализацию и переиспользование сообщений об ошибках.
        [EmailAddress(ErrorMessage = "Invalid {0} format")]
        [StringLength(EmailMaxLength, MinimumLength = 5, ErrorMessage = "{0} must be 5-{1} characters")]
        public string Email { get; set; } = "";

        // Запрет ':', '"' и пробелов в имени пользователя для Basic Auth, защиты от инъекций и чистоты данных
        [Display(Name = "Username")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(MaxLength, MinimumLength = MinLength, ErrorMessage = "{0} must be {2}-{1} characters")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "{0} can only contain letters, numbers, '.', '_', or '-'")]
        public string Username { get; set; } = "";

        [Display(Name = "Phone")]
        [Phone(ErrorMessage = "Invalid {0} format")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(MaxLength, MinimumLength = PasswordMinLength, ErrorMessage = "{0} must be {2}-{1} characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "{0} must include uppercase, lowercase, and a number")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Password Repeat")]
        [Required(ErrorMessage = "{0} is required")]
        [StringLength(MaxLength, MinimumLength = PasswordMinLength, ErrorMessage = "{0} must be {2}-{1} characters")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string PasswordRepeat { get; set; } = "";
    }
}
