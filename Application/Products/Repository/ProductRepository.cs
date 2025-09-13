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
            if (!string.IsNullOrWhiteSpace(product.Slug) && await IsSlugExistAsync(product.Slug))
                throw new InvalidOperationException($"Slug '{product.Slug}' already taken.");


            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existing == null) 
                throw new KeyNotFoundException($"Product '{product.Id}' not found.");

            if (!string.IsNullOrWhiteSpace(product.Slug))
            {
                var slugTaken = await IsSlugExistAsync(product.Slug, product.Id);
                if (slugTaken)
                    throw new InvalidOperationException($"Slug '{product.Slug}' already taken.");
                existing.Slug = product.Slug;
            }    


            existing.Title          = product.Title;
            //existing.Slug           = product.Slug;
            existing.ImgUrl         = product.ImgUrl;
            existing.Price          = product.Price;
            existing.DiscountPrice  = product.DiscountPrice;
            existing.IsRecommended  = product.IsRecommended;
            existing.IsBest         = product.IsBest;

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
            if (pageSize > 100) pageSize = 100;        //  ограничим 100?

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

        public Task<bool> IsSlugExistAsync(string slug, Guid? exceptId = null)
        {
            slug = (slug ?? "").Trim().ToLowerInvariant();
            return _db.Products.AnyAsync(p => p.Slug == slug && (!exceptId.HasValue || p.Id != exceptId.Value));
        }

        // TODO: Потом проверить...
        public async Task SetCategoriesAsync(Guid productId, IReadOnlyCollection<Guid> categoryIds)
        {
            var product = await _db.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == productId)
                ?? throw new KeyNotFoundException($"Product '{productId}' not found.");

            var newSet = new HashSet<Guid>(categoryIds ?? Array.Empty<Guid>());
            var curSet = new HashSet<Guid>(product.Categories.Select(c => c.Id));

            // удалить лишние связи
            var toRemove = product.Categories.Where(c => !newSet.Contains(c.Id)).ToList();
            foreach (var c in toRemove) product.Categories.Remove(c);

            // добавить недостающие связи
            var toAdd = newSet.Except(curSet);
            foreach (var id in toAdd)
            {
                // проверим что категория существует
                var stub = new Category { Id = id };
                _db.Attach(stub);                 // пометит как Unchanged, EF создаст только связку в M2M
                product.Categories.Add(stub);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<Guid>> GetExistingCategoryIdsAsync(IReadOnlyCollection<Guid> ids)    // у прдукта может быть несколько категорий
        {
            if (ids == null || ids.Count == 0) return new List<Guid>();
            return await _db.Categories
                .Where(c => ids.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();
        }

    }
}
