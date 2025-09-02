using System.ComponentModel.DataAnnotations;

namespace rozetochka_api.Domain.Entities
{
    public class Product
    {
        public Guid Id  { get; set; }
        public string   Title  { get; set; } = "";
        public string   Slug   { get; set; } = "";      // slug = href
        public string   ImgUrl { get; set; } = "";
        public decimal  Price { get; set; }
        public decimal? DiscountPrice { get; set; }

        public bool     IsRecommended { get; set; }     // для home recommendProducts ???
        public bool     IsBest        { get; set; }     // для home bestProducts ??? 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();   // картинки продукта
        

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }



}
