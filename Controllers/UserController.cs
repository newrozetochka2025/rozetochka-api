using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Application.Users.DTOs.Examples;
using rozetochka_api.Application.Users.Service;
using rozetochka_api.Shared;
using rozetochka_api.Shared.Extensions;
using rozetochka_api.Shared.Helpers;
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

        // POST     /api/user/register -> Data: UserResponseDto (201 Created)
        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerRequestExample(typeof(UserRegisterRequestDto), typeof(UserRegisterRequestExample))]
        public async Task<ActionResult<RestResponse>> Register([FromBody] UserRegisterRequestDto request)
        {
            _logger.LogInformation("Registration request received for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToErrorDictionary();

                _logger.LogWarning("Validation failed for registration request: {Errors}", string.Join(", ", errors.SelectMany(kvp => kvp.Value)));

                var (statusCode, phrase) = HttpErrorMapping.Get("VALIDATION_ERROR");

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
                    var (statusCode, phrase) = HttpErrorMapping.Get(result.ErrorCode);

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


        // POST     /api/user/login  -> Data: { accessToken, refreshToken, expiresIn, user } (200 OK)
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<RestResponse>> Login([FromBody] UserLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var (code, phrase) = HttpErrorMapping.Get("VALIDATION_ERROR");
                return StatusCode(code, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = code, Phrase = phrase },
                    Meta = new RestMeta { Service = "User Login", Method = "POST", Action = "/api/user/login", DataType = "validation" },
                    Data = ModelState.ToErrorDictionary()
                });
            }

            var result = await _userService.LoginAsync(request);
            if (!result.IsSuccess)
            {
                var (code, phrase) = HttpErrorMapping.Get(result.ErrorCode ?? "INVALID_CREDENTIALS");
                return StatusCode(code, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = code, Phrase = phrase },
                    Meta = new RestMeta { Service = "User Login", Method = "POST", Action = "/api/user/login", DataType = "string" },
                    Data = result.ErrorMessage
                });
            }

            return Ok(new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 200, Phrase = "OK" },
                Meta = new RestMeta { Service = "User Login", Method = "POST", Action = "/api/user/login", DataType = "auth_response" },
                Data = result.Data // AuthResponseDto (accessToken, refreshToken, expiresIn, user)
            });
        }

        // POST     /api/user/refresh -> Data: { accessToken, refreshToken, expiresIn, user } (200 OK)
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<RestResponse>> Refresh([FromBody] RefreshRequestDto request)
        {
            var result = await _userService.RefreshAsync(request.RefreshToken);
            if (!result.IsSuccess)
            {
                const int code = 401;
                return StatusCode(code, new RestResponse
                {
                    Status = new RestStatus { IsOk = false, Code = code, Phrase = "Unauthorized" },
                    Meta = new RestMeta { Service = "User Refresh", Method = "POST", Action = "/api/user/refresh", DataType = "string" },
                    Data = result.ErrorMessage
                });
            }

            return Ok(new RestResponse
            {
                Status = new RestStatus { IsOk = true, Code = 200, Phrase = "OK" },
                Meta = new RestMeta { Service = "User Refresh", Method = "POST", Action = "/api/user/refresh", DataType = "jwt" },
                Data = result.Data  // AuthResponseDto
            });
        }



    }
}
