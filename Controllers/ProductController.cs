using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Products.DTOs;
using rozetochka_api.Application.Products.Service;
using rozetochka_api.Shared;
using rozetochka_api.Shared.Extensions;
using rozetochka_api.Shared.Helpers;
using System.Security.Claims;

namespace rozetochka_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }


        // POST /api/product
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RestResponse>> Create([FromBody] ProductCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToErrorDictionary();
                var (statusCode, phrase) = HttpErrorMapping.Get("VALIDATION_ERROR");

                _logger.LogWarning("Product create validation failed: {Errors}", string.Join(", ", errors.SelectMany(kvp => kvp.Value)));

                var errorResponse = new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = statusCode, Phrase = phrase },
                    Meta = new RestMeta
                    {
                        Service = "Product Create",
                        Method = "POST",
                        Action = "/api/product",
                        DataType = "dictionary",
                        Params = new Dictionary<string, object> { ["request"] = request }
                    },
                    Data = errors
                };
                return StatusCode(statusCode, errorResponse);
            }

            // ownerId c JWT
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(sub, out var ownerId))
            {
                const int code = 401;
                return StatusCode(code, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = code, Phrase = "Unauthorized" },
                    Meta = new RestMeta { Service = "Product Create", Method = "POST", Action = "/api/product", DataType = "string" },
                    Data = "Invalid token"
                });
            }

            var result = await _productService.CreateAsync(request, ownerId);

            if (!result.IsSuccess)
            {
                var (statusCode, phrase) = HttpErrorMapping.Get(result.ErrorCode);

                return StatusCode(statusCode, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = statusCode, Phrase = phrase },
                    Meta = new RestMeta
                    {
                        Service = "Product Create",
                        Method = "POST",
                        Action = "/api/product",
                        DataType = "string",
                        Params = new Dictionary<string, object> { ["request"] = request }
                    },
                    Data = result.ErrorMessage
                });
            }

            return StatusCode(201, new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 201, Phrase = "Created" },
                Meta = new RestMeta { Service = "Product Create", Method = "POST", Action = "/api/product", DataType = "product" },
                Data = result.Data
            });
        }


    }
}
