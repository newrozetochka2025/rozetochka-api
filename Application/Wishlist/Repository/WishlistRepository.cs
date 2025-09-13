using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Data;

namespace rozetochka_api.Application.Wishlist.Repository
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _db;
        public WishlistRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public Task<bool> IsInWishlistAsync(Guid userId, Guid productId)
        {
            return _db.WishlistItems.AnyAsync(x => x.UserId == userId && x.ProductId == productId);
        }

        public async Task AddAsync(Guid userId, Guid productId)
        {
            var exists = await IsInWishlistAsync(userId, productId);
            if (exists) return;

            await _db.WishlistItems.AddAsync(new WishlistItem { UserId = userId, ProductId = productId });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid userId, Guid productId)
        {
            var entity = await _db.WishlistItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
            if (entity == null) return;

            _db.WishlistItems.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ToggleAsync(Guid userId, Guid productId)
        {
            var entity = await _db.WishlistItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
            if (entity == null)
            {
                await _db.WishlistItems.AddAsync(new WishlistItem { UserId = userId, ProductId = productId });
                await _db.SaveChangesAsync();
                return true;
            }

            _db.WishlistItems.Remove(entity);
            await _db.SaveChangesAsync();
            return false;
        }

        public async Task<List<Guid>> GetProductIdsForUserAsync(Guid userId)
        {
            return await _db.WishlistItems
                .Where(x => x.UserId == userId)
                .Select(x => x.ProductId)
                .ToListAsync();
        }
    }
}
