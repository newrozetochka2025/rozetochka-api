namespace rozetochka_api.Domain.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string IconSvgUrl { get; set; } = "";    // svg
        public string Slug { get; set; } = "";          // slug = href
        public string ImageUrl { get; set; } = "";      // NOT NULL ?


        public Guid? ParentId { get; set; }         // FK на родительскую категорию,  null = корневая
        public Category? Parent { get; set; }       // нав. свойствоа на parent
        public ICollection<Category> Children { get; set; } = new List<Category>();     // нав.  Дочерние под-категории
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}