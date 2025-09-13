using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Banners.Service;
using rozetochka_api.Application.Categories.Service;
using rozetochka_api.Application.Products.Service;
using rozetochka_api.Application.Wishlist.Repository;
using rozetochka_api.Shared;
using System.Security.Claims;

namespace rozetochka_api.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ICategoryService _categories;
        private readonly IProductService _products;
        private readonly IBannerService _banners;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ICategoryService categories,
            IProductService products,
            IBannerService banners,
            ILogger<HomeController> logger)
        {
            _categories = categories;
            _products = products;
            _banners = banners;
            _logger = logger;
        }


        // GET      /api/home?limit=25
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<RestResponse>> Get([FromQuery] int limit = 25)
        {
            if (limit <= 0) limit = 25;

            Guid? userId = null;
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(sub) && Guid.TryParse(sub, out var uid))
            {
                userId = uid;
            }

            var categories  = await _categories.GetAllAsync();
            var banners     = await _banners.GetAllAsync();
            var rec         = await _products.GetRecommendedAsync(limit, userId);
            var best        = await _products.GetBestAsync(limit, userId);

            var data = new
            {
                categories,
                banners,
                recommendProducts = rec,
                bestProducts = best
            };

            return Ok(new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 200, Phrase = "OK" },
                Meta = new RestMeta { Service = "Home", Method = "GET", Action = "/api/home", DataType = "home" },
                Data = data
            });

        }
    }
}
