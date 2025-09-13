using rozetochka_api.Application.Banners.DTOs;

namespace rozetochka_api.Application.Banners.Service
{
    public interface IBannerService
    {
        Task<List<BannerResponseDto>> GetAllAsync();
    }
}
