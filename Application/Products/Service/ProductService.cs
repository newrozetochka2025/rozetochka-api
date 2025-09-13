using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using rozetochka_api.Application.Products.DTOs;
using rozetochka_api.Application.Products.Repository;
using rozetochka_api.Application.Wishlist.Repository;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Products.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            IWishlistRepository wishlistRepository,
            IMapper mapper, 
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _wishlistRepository = wishlistRepository;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<ServiceResult<ProductResponseDto>> CreateAsync(ProductCreateDto dto, Guid ownerId)
        {
            try
            {
                var id = Guid.NewGuid();

                if (dto.CategoryIds is { Count: > 0 })
                {
                    var requested = dto.CategoryIds.Distinct().ToList();
                    // вытащим существующие Id из БД
                    var existing = await _productRepository.GetExistingCategoryIdsAsync(requested);
                    var missing = requested.Except(existing).ToList();
                    if (missing.Count > 0)
                    {
                        // вернём читаемую ошибку с конкретными ID
                        return ServiceResult<ProductResponseDto>.Fail(
                            $"Categories not found: {string.Join(", ", missing)}",
                            "CATEGORY_NOT_FOUND");
                    }
                }

                // без автомапера для контроля и явности
                var entity = new Product
                {
                    Id = id,
                    Title = dto.Title.Trim(),
                    Slug = id.ToString("n"),   // slug = id
                    ImgUrl = dto.ImgUrl,
                    Price = dto.Price,
                    DiscountPrice = dto.DiscountPrice,
                    CreatedAt = DateTime.UtcNow,
                    IsRecommended = false,
                    IsBest = false,
 
                    OwnerId = ownerId
                };

                // галерея iamges (если передали)
                if (dto.ImageUrls is { Count: > 0 })
                {
                    entity.Images = dto.ImageUrls.Select(u => new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = id,
                        Url = u
                    }).ToList();
                }

                await _productRepository.AddAsync(entity);

                // привязка категорий (если передали?)
                if (dto.CategoryIds is { Count: > 0 })
                {
                    await _productRepository.SetCategoriesAsync(id, dto.CategoryIds);
                }

                var dtoOut = _mapper.Map<ProductResponseDto>(entity);
                return ServiceResult<ProductResponseDto>.Ok(dtoOut);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number == 547)
            {
                _logger.LogError(ex, "FK violation while creating product (owner/categories). OwnerId: {OwnerId}", ownerId);
                return ServiceResult<ProductResponseDto>.Fail("Foreign key violation (owner or category).", "FOREIGN_KEY_VIOLATION");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateAsync failed for owner {OwnerId}", ownerId);
                return ServiceResult<ProductResponseDto>.Fail("Failed to create product", "CREATE_FAILED");
            }
        }


        public async Task<List<ProductResponseDto>> GetRecommendedAsync(int limit = 25, Guid? userId = null)
        {
            var items = await _productRepository.GetRecommendedAsync(limit);

            HashSet<Guid>? wish = null;
            if (userId.HasValue)
            {
                var ids = await _wishlistRepository.GetProductIdsForUserAsync(userId.Value);
                wish = new HashSet<Guid>(ids);
            }

            return _mapper.Map<List<ProductResponseDto>>(items, opts =>
            {
                if (wish != null) opts.Items["wishlist"] = wish;
            });
        }


        public async Task<List<ProductResponseDto>> GetBestAsync(int limit = 25, Guid? userId = null)
        {
            var items = await _productRepository.GetBestAsync(limit);

            HashSet<Guid>? wish = null;
            if (userId.HasValue)
            {
                var ids = await _wishlistRepository.GetProductIdsForUserAsync(userId.Value);
                wish = new HashSet<Guid>(ids);
            }

            return _mapper.Map<List<ProductResponseDto>>(items, opts =>
            {
                if (wish != null) opts.Items["wishlist"] = wish;
            });
        }


        public async Task<PagedResult<ProductResponseDto>> GetByCategoryIdPagedAsync(
                            Guid categoryId, int page, int pageSize, string? sort = null)
        {
            var paged = await _productRepository.GetByCategoryIdPagedAsync(categoryId, page, pageSize, sort);
            return new PagedResult<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(paged.Items),
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalItems = paged.TotalItems
            };
        }
    }
}
