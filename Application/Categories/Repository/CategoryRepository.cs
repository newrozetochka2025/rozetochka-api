using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Data;

namespace rozetochka_api.Application.Categories.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }



        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()    // Без подкатегорий
        {
            return await _db.Categories.AsNoTracking().ToListAsync();
        }
    
        public async Task<Category?> GetCategoryById(Guid id)
        {
            return await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }  

        public async Task<IEnumerable<Category>> GetCategoriesWithChildrenAsync()    // Корневые категории + подкатегории (1 уровень) 
        {
            return await _db.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Children)
                .AsSplitQuery()         // <- отдельные запросы для родителей и детей, без дублей от JOIN
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoryTreeAsync()               // Полное дерево категорий (все уровни)
        {
            var all = await _db.Categories.AsNoTracking().ToListAsync();

            var map = all.ToDictionary(c => c.Id);
            var roots = new List<Category>();

            foreach (var c in all)
            {
                if (c.ParentId is Guid pid && map.TryGetValue(pid, out var parent))
                {
                    parent.Children.Add(c);
                }
                else { roots.Add(c); }  // если ParentId == null, это корневая категория
            }

            return roots;
        }

        public async Task<Category?> GetCategoryWithProductsById(Guid id)         // Категория + продукты (+ изображения).
        {
            return await _db.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Images)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var entity = await _db.Categories.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Category '{id}' not found.");

            // Category имеет self reference через ParentId.
            // Удаление родителя с дочерними категориями запрещено (DeleteBehavior.Restrict).
            var hasChildren = await _db.Categories.AnyAsync(c => c.ParentId == id);
            if (hasChildren)
                throw new InvalidOperationException("Cannot delete a category that has child categories.");

            _db.Categories.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            var existing = await _db.Categories.FirstOrDefaultAsync(c => c.Id == category.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Category '{category.Id}' not found.");

            existing.Title = category.Title;
            existing.IconSvgUrl = category.IconSvgUrl;
            existing.Slug = category.Slug;
            existing.ImageUrl = category.ImageUrl;
            existing.ParentId = category.ParentId;

            await _db.SaveChangesAsync();
        }
    }
}
