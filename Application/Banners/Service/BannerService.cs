using AutoMapper;
using rozetochka_api.Application.Banners.DTOs;
using rozetochka_api.Application.Banners.Repository;

namespace rozetochka_api.Application.Banners.Service
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public BannerService(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<List<BannerResponseDto>> GetAllAsync()
        {
            var items = await _bannerRepository.GetAllAsync();
            return _mapper.Map<List<BannerResponseDto>>(items);
        }
    }
}
