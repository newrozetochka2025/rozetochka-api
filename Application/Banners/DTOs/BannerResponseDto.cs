namespace rozetochka_api.Application.Banners.DTOs
{
    public class BannerResponseDto
    {
        public Guid Id { get; set; }
        public string Img { get; set; } = "";        // ImgUrl
        public string Href { get; set; } = "";
    }
}
