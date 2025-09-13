namespace rozetochka_api.Application.Categories.DTOs
{
    public class CategoryResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Svg { get; set; } = "";   // IconSvgUrl
        public string Href { get; set; } = "";   // Slug
        public string ImageUrl { get; set; } = "";
    }
}
