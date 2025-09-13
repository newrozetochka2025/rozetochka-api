using rozetochka_api.Application.Categories.DTOs;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Categories.Service
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync();
        Task<ServiceResult<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto);
    }
}
