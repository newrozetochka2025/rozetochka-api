using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Categories.Repository
{
    public interface ICategoryRepository
    {
        
        Task<IEnumerable<Category>> GetAllCategoriesAsync();            // Все категории (без include)
        Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync();   // Корневые категории + подкатегории (1 уровень)
        Task<IEnumerable<Category>> GetCategoryTreeAsync();             // Полное дерево категорий (все уровни)

        Task<Category?> GetCategoryById(Guid id);
        Task<Category?> GetCategoryWithProductsById(Guid id);

        
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid id);
        Task UpdateCategoryAsync(Category category);


        Task<bool> IsSlugExistAsync(string slug, Guid? exceptId = null);
    }
}
