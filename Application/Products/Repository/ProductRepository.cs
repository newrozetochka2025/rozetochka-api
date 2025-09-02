using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Data;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Products.Repository
{
    public class ProductRepository :  IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) 
        {
            _db = db;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetRecommendedAsync(int limit = 25)
        {
            if (limit <= 0) limit = 25;

            return await _db.Products
                .Where(p => p.IsRecommended)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetBestAsync(int limit = 25)
        {
            if (limit <= 0) limit = 25;

            return await _db.Products
                .Where(p => p.IsBest)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _db.Products
                .Include(p => p.Images)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task AddAsync(Product product)
        {
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Product '{product.Id}' not found.");

            existing.Title = product.Title;
            existing.Slug = product.Slug;
            existing.ImgUrl = product.ImgUrl;
            existing.Price = product.Price;
            existing.DiscountPrice = product.DiscountPrice;
            existing.IsRecommended = product.IsRecommended;
            existing.IsBest = product.IsBest;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.Products.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Product '{id}' not found.");

            _db.Products.Remove(entity);
            await _db.SaveChangesAsync();
        }


        // --- По категории c пагинацией ---

        public async Task<PagedResult<Product>> GetByCategoryIdPagedAsync(
            Guid categoryId, int page, int pageSize, string? sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;          // 

            var baseQuery = _db.Products.Where(p => p.Categories.Any(c => c.Id == categoryId));

            var total = await baseQuery.CountAsync();

            var query = sort switch
            {
                "price_asc" => baseQuery.OrderBy(p => p.Price),
                "price_desc" => baseQuery.OrderByDescending(p => p.Price),
                _ => baseQuery.OrderByDescending(p => p.CreatedAt)
            };

            // подгружаем картинки для карточек
            var items = await query
                .Include(p => p.Images)
                .AsSplitQuery()
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }


    }
}
