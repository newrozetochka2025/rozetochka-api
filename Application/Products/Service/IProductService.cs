using rozetochka_api.Application.Products.DTOs;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Products.Service
{
    public interface IProductService
    {
        // ownerId из JWT в контроллере
        Task<ServiceResult<ProductResponseDto>> CreateAsync(ProductCreateDto dto, Guid ownerId);


        // userId опционально: если передали — отметим wishlist
        Task<List<ProductResponseDto>> GetRecommendedAsync(int limit = 25, Guid? userId = null);
        Task<List<ProductResponseDto>> GetBestAsync(int limit = 25, Guid? userId = null);

        Task<PagedResult<ProductResponseDto>> GetByCategoryIdPagedAsync(
            Guid categoryId, int page, int pageSize, string? sort = null);
    }
}
