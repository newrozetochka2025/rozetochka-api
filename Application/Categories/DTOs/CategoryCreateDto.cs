using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Application.Categories.DTOs
{
    public class CategoryCreateDto
    {

        [Required, StringLength(256)]
        public required string Title { get; set; }

        [Required, StringLength(1024)] 
        public required string IconSvgUrl { get; set; }

        [Required, StringLength(256)]
        [RegularExpression("^[a-z0-9-]*$", ErrorMessage = "Slug: only a-z, 0-9, '-'")]
        public required string Slug { get; set; }

        [Required, StringLength(1024)] 
        public required string ImageUrl { get; set; }

    }
}
