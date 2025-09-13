using AutoMapper;
using Microsoft.Extensions.Logging;
using rozetochka_api.Application.Categories.DTOs;
using rozetochka_api.Application.Categories.Repository;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Categories.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ILogger<CategoryService> logger,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<List<CategoryResponseDto>> GetAllAsync()  // Без подкатегорий
        {
            var entities = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.Map<List<CategoryResponseDto>>(entities);
        }


        public async Task<ServiceResult<CategoryResponseDto>> CreateAsync(CategoryCreateDto dto)
        {
            try
            {
                var slug = dto.Slug.Trim().ToLowerInvariant();
                if (await _categoryRepository.IsSlugExistAsync(slug))
                {
                    return ServiceResult<CategoryResponseDto>.Fail("Slug already taken", "SLUG_EXISTS");
                }
                    
                var entity = new Category
                {
                    Id          = Guid.NewGuid(),
                    Title       = dto.Title.Trim(),
                    Slug        = slug,
                    IconSvgUrl  = dto.IconSvgUrl.Trim(),
                    ImageUrl    = dto.ImageUrl.Trim(),
                };

                await _categoryRepository.AddCategoryAsync(entity);
                return ServiceResult<CategoryResponseDto>.Ok(_mapper.Map<CategoryResponseDto>(entity));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Create category failed");
                return ServiceResult<CategoryResponseDto>.Fail("Create category failed", "CREATE_FAILED");
            }
        }

    }
}
