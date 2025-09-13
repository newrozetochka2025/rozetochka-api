using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Categories.DTOs;
using rozetochka_api.Application.Categories.Service;
using rozetochka_api.Shared;
using System.Text.Json;
using rozetochka_api.Shared.Extensions;
using rozetochka_api.Shared.Helpers;

namespace rozetochka_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // POST /api/category
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<RestResponse>> Create([FromBody] CategoryCreateDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToErrorDictionary();
                var (statusCode, phrase) = HttpErrorMapping.Get("VALIDATION_ERROR");

                _logger.LogWarning("Category create validation failed: {Errors}", string.Join(", ", errors.SelectMany(kvp => kvp.Value)));

                var errorResponse = new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = statusCode, Phrase = phrase },
                    Meta = new RestMeta
                    {
                        Service = "Category Create",
                        Method = "POST",
                        Action = "/api/category",
                        DataType = "dictionary",
                        Params = new Dictionary<string, object> { ["request"] = request }
                    },
                    Data = errors
                };
                return StatusCode(statusCode, errorResponse);
            }

            var result = await _categoryService.CreateAsync(request);
            if (!result.IsSuccess)
            {
                var (statusCode, phrase) = HttpErrorMapping.Get(result.ErrorCode);

                return StatusCode(statusCode, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = statusCode, Phrase = phrase },
                    Meta = new RestMeta { Service = "Category Create", Method = "POST", Action = "/api/category", DataType = "string" },
                    Data = result.ErrorMessage
                });
            }

            return StatusCode(201, new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 201, Phrase = "Created" },
                Meta = new RestMeta { Service = "Category Create", Method = "POST", Action = "/api/category", DataType = "category" },
                Data = result.Data
            });
        }




        


        



  
        

    }
}
