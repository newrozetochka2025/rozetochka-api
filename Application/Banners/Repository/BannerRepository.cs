using Microsoft.EntityFrameworkCore;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Data;

namespace rozetochka_api.Application.Banners.Repository
{
    public class BannerRepository : IBannerRepository
    {
        private readonly ApplicationDbContext _db;

        public BannerRepository(ApplicationDbContext db)
        {
            _db = db;
        }


        public async Task<IEnumerable<Banner>> GetAllAsync()
        {
            return await _db.Banners.AsNoTracking().ToListAsync();
        }

        public Task<Banner?> GetByIdAsync(Guid id)
        {
            return _db.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Banner banner)
        {
            await _db.Banners.AddAsync(banner);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Banner banner)
        {
            var existing = await _db.Banners.FirstOrDefaultAsync(b => b.Id == banner.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Banner '{banner.Id}' not found.");

            existing.ImgUrl = banner.ImgUrl;
            existing.Href = banner.Href;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _db.Banners.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Banner '{id}' not found.");

            _db.Banners.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}
