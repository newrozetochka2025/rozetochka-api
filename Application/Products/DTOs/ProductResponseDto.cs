namespace rozetochka_api.Application.Products.DTOs
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Href { get; set; } = "";   // Slug
        public string Img { get; set; } = "";   // ImgUrl
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool IsInWishlist { get; set; } = false;     // заглушка
    }
}
