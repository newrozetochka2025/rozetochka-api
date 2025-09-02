using rozetochka_api.Domain.Entities;

namespace rozetochka_api.Application.Banners.Repository
{
    public interface IBannerRepository
    {
        Task<IEnumerable<Banner>> GetAllAsync();
        Task<Banner?> GetByIdAsync(Guid id);
        Task AddAsync(Banner banner);
        Task UpdateAsync(Banner banner);
        Task DeleteAsync(Guid id);
    }
}
