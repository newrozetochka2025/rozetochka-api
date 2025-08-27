using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Application.Users.DTOs.Examples;
using rozetochka_api.Application.Users.Service;
using rozetochka_api.Shared;
using Swashbuckle.AspNetCore.Filters;

namespace rozetochka_api.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;


        public UserController(IUserService userService, ILogger<UserController> logger, IMapper mapper)
        {
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("register")]
        [SwaggerRequestExample(typeof(UserRegisterRequestDto), typeof(UserRegisterRequestExample))]
        public async Task<ActionResult<RestResponse>> Register([FromBody] UserRegisterRequestDto request)
        {
            _logger.LogInformation("Registration request received for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    );

                _logger.LogWarning("Validation failed for registration request: {Errors}", string.Join(", ", errors.SelectMany(kvp => kvp.Value)));

                var (statusCode, phrase) = GetErrorResponse("VALIDATION_ERROR");

                var errorResponse = new RestResponse
                {
                    Status = new RestStatus
                    {
                        IsOk = false,
                        Code = statusCode,
                        Phrase = phrase
                    },
                    Meta = new RestMeta
                    {
                        Service = "User Register",
                        Method = "POST",
                        Action = "/api/user/register",
                        DataType = "dictionary",
                        Params = new Dictionary<string, object>
                        {
                            { "Email", request.Email },
                            { "Username", request.Username }
                        }
                    },
                    Data = errors
                };
                return StatusCode(statusCode, errorResponse);
            }

            try
            {
                var result = await _userService.RegisterUserAsync(request);

                if (!result.IsSuccess)
                {
                    var (statusCode, phrase) = GetErrorResponse(result.ErrorCode);

                    var errorResponse = new RestResponse
                    {
                        Status = new RestStatus
                        {
                            IsOk = false,
                            Code = statusCode,
                            Phrase = phrase
                        },
                        Meta = new RestMeta
                        {
                            Service = "User Register",
                            Method = "POST",
                            Action = "/api/user/register",
                            DataType = "string",
                            Params = new Dictionary<string, object>
                            {
                                { "Email", request.Email },
                                { "Username", request.Username }
                            }
                        },
                        Data = result.ErrorMessage
                    };

                    return StatusCode(statusCode, errorResponse);
                }

                var successResponse = new RestResponse
                {
                    Status = new RestStatus
                    {
                        IsOk = true,
                        Code = 201,
                        Phrase = "Created"
                    },
                    Meta = new RestMeta
                    {
                        Service = "User Register",
                        Method = "POST",
                        Action = "/api/user/register",
                        DataType = "user",
                    },
                    Data = result.Data
                };

                return StatusCode(successResponse.Status.Code, successResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user registration for email: {Email}", request.Email);
                throw;      // Позволяем глобальному обработчику исключений обработать это ???
                            // ИЛИ СДЕЛАТЬ ЕЩЕ ОДИН РЕСПОНС?
            }
        }



        private static (int statusCode, string phrase) GetErrorResponse(string? errorCode)
        {
            return errorCode switch
            {
                "EMAIL_TAKEN" => (409, "Conflict"),
                "USERNAME_TAKEN" => (409, "Conflict"),
                "VALIDATION_ERROR" => (422, "Unprocessable Entity"),
                "INVALID_EMAIL" => (422, "Unprocessable Entity"),
                "INVALID_USERNAME" => (422, "Unprocessable Entity"),
                "INVALID_PASSWORD" => (422, "Unprocessable Entity"),
                "PASSWORDS_DONT_MATCH" => (422, "Unprocessable Entity"),
                "DATABASE_ERROR" => (500, "Internal Server Error"),
                _ => (400, "Bad Request")
            };
        }
    }
}
