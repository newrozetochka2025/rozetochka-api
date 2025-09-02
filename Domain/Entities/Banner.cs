namespace rozetochka_api.Domain.Entities
{
    public class Banner
    {
        public Guid Id { get; set; }
        public string ImgUrl { get; set; } = "";    // Url картинки
        public string Href { get; set; } = "";      // ссылка на рекламируемый товар баннера.
    }
}
