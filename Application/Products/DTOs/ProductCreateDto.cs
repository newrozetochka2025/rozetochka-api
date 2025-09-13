using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Application.Products.DTOs
{
    public class ProductCreateDto
    {
        // OwnerId из DTO не принимаем — берём из аутентифицированного пользователя (JWT)

        [Required, StringLength(256)]
        public required string Title { get; set; }


        [StringLength(256)]
        [RegularExpression("^[a-z0-9-]+$", ErrorMessage = "Slug: only a-z, 0-9, '-'")]
        public string? Slug { get; set; }     // если User создает, то slug = null и возвращается id в поле slug через mapper


        [Required, StringLength(1024)]
        public required string ImgUrl { get; set; }


        [Required, Range(0, double.MaxValue)]
        public decimal Price { get; set; }


        [Range(0, double.MaxValue)]
        public decimal? DiscountPrice { get; set; }


        public List<string>? ImageUrls { get; set; }        // 

        [MinLength(1, ErrorMessage = "At least one category is required")]
        public List<Guid> CategoryIds { get; set; } = new();    // id  ??
                 
    }
}
