using rozetochka_api.Domain.Entities;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Products.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();       // Убрать ? тк могут быть тысячи продуктов...лучше по пагинации.
        Task<IEnumerable<Product>> GetRecommendedAsync(int limit = 25);   // IsRecommended = true
        Task<IEnumerable<Product>> GetBestAsync(int limit = 25);          // IsBest = true

        // --- По категории ---
        Task<PagedResult<Product>> GetByCategoryIdPagedAsync(               // с пагинацией
            Guid categoryId, int page, int pageSize, string? sort = null);  


        Task<Product?> GetByIdAsync(Guid id);               // + Images
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);

        Task<List<Guid>> GetExistingCategoryIdsAsync(IReadOnlyCollection<Guid> ids);    // существующие id категорий у продукта
        Task<bool> IsSlugExistAsync(string slug, Guid? exceptId = null);
        Task SetCategoriesAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds);
    }
}
