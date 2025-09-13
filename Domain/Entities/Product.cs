using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Domain.Entities
{
    public class Product
    {
        public Guid Id  { get; set; }
        public required string  Title  { get; set; }
        public required string  Slug   { get; set; }            // slug = href     // убрал nullable тк в сервисе ложу туда product.id?
        public required string  ImgUrl { get; set; }            //  основная картинка?
        public decimal  Price { get; set; }
        public decimal? DiscountPrice { get; set; }


        public bool     IsRecommended { get; set; }     // для home recommendProducts ???
        public bool     IsBest        { get; set; }     // для home bestProducts ??? 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();   // картинки продукта
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        // Владелец product
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;
    }



}
