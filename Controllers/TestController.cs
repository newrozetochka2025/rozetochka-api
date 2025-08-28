using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Shared;

namespace rozetochka_api.Controllers
{

    // Test контроллер для "тестовых" API запросов

    // GET /api/test/user
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("user")]
        public ActionResult<RestResponse> GetUser()
        {
            var user = new UserResponseDto
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com",
                Username = "tester",
                Phone = "+380967775533",
                Role = "user",
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // Unix time (сек)
            };

            var resp = new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 200, Phrase = "OK" },
                Meta = new RestMeta
                {
                    Service = "test",
                    Method = "GET",
                    Action = "/api/test/user",
                    DataType = "user"
                },
                Data = user
            };

            return Ok(resp);
        }
    }
}
