using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required, MaxLength(1024)]
        public string Url { get; set; } = "";

    }
}
