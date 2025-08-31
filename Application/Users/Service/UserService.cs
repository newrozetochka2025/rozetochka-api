using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Application.Users.Repository;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Identity.Interfaces;
using rozetochka_api.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace rozetochka_api.Application.Users.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            ILogger<UserService> logger,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
            _config = config;
        }

        public async Task<ServiceResult<UserResponseDto>> RegisterUserAsync(UserRegisterRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var username = dto.Username.Trim();
            var phone = dto.Phone?.Trim() ?? "";


            _logger.LogInformation("Register attempt: email={Email}, username={Username}", email, username);

            try
            {
                if (await _userRepository.IsEmailTakenAsync(email))
                {
                    _logger.LogWarning("Registration failed: email {Email} is already taken", email);
                    return ServiceResult<UserResponseDto>.Fail("Email already taken", "EMAIL_TAKEN");
                }

                if (await _userRepository.IsUsernameTakenAsync(username))
                {
                    _logger.LogWarning("Registration failed: username {Username} is already taken", username);
                    return ServiceResult<UserResponseDto>.Fail("Username already taken", "USERNAME_TAKEN");
                }

                // маппинг и хэш
                var user = _mapper.Map<User>(dto);
                user.Email = email;
                user.Username = username;
                user.Phone = phone;
                user.PasswordHash = _passwordHasher.HashPassword(dto.Password);

                await _userRepository.AddUserAsync(user);

                _logger.LogInformation("User created: id={UserId}, email={Email}", user.Id, user.Email);

                // на выходе DTO
                var userDto = _mapper.Map<UserResponseDto>(user);
                return ServiceResult<UserResponseDto>.Ok(userDto);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException on register: email={Email}, username={Username}", email, username);
                return ServiceResult<UserResponseDto>.Fail("Database update error", "DATABASE_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on register: email={Email}", email);
                return ServiceResult<UserResponseDto>.Fail("An unexpected error occurred", "UNEXPECTED_ERROR");
            }
        }

        // LOGIN
        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(UserLoginRequestDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            _logger.LogInformation("Login attempt: {Email}", email);

            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                var valid = user != null && _passwordHasher.VerifyPassword(dto.Password, user.PasswordHash);
                if (!valid)
                    return ServiceResult<AuthResponseDto>.Fail("Invalid credentials", "INVALID_CREDENTIALS");

                // аксес + время жизни
                var (access, expiresIn) = GenerateJwtWithTtl(user);

                // одноразовый refresh на 7 дней (в БД)
                var refresh = new UserRefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = GenerateSecureToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                await _userRepository.AddRefreshTokenAsync(refresh);

                return ServiceResult<AuthResponseDto>.Ok(new AuthResponseDto
                {
                    AccessToken = access,
                    RefreshToken = refresh.Token,
                    ExpiresIn = expiresIn,
                    User = _mapper.Map<UserResponseDto>(user)
                });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException on login: {Email}", email);
                return ServiceResult<AuthResponseDto>.Fail("Database update error", "DATABASE_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on login: {Email}", email);
                return ServiceResult<AuthResponseDto>.Fail("An unexpected error occurred", "UNEXPECTED_ERROR");
            }
        }

        // REFRESH
        public async Task<ServiceResult<AuthResponseDto>> RefreshAsync(string refreshToken)
        {
            _logger.LogInformation("Refresh attempt");

            try
            {
                var stored = await _userRepository.GetRefreshTokenAsync(refreshToken);
                // Проверяем существование + срок жизни
                if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
                    return ServiceResult<AuthResponseDto>.Fail("Invalid refresh token", "INVALID_REFRESH");

                var user = stored.User;

                // Отзываем токен (token.IsRevoked = true),
                // Старые записи будут копиться в БД — их стоит периодически очищать, например раз в месяц (IsRevoked = true && ExpiresAt < Now).
                await _userRepository.RevokeRefreshTokenAsync(stored);

                // Новые access/refresh
                var (newAccess, expiresIn) = GenerateJwtWithTtl(user);

                var newRefresh = new UserRefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = GenerateSecureToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                };
                await _userRepository.AddRefreshTokenAsync(newRefresh);

                return ServiceResult<AuthResponseDto>.Ok(new AuthResponseDto
                {
                    AccessToken = newAccess,
                    RefreshToken = newRefresh.Token,
                    ExpiresIn = expiresIn,
                    User = _mapper.Map<UserResponseDto>(user)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error on refresh");
                return ServiceResult<AuthResponseDto>.Fail("An unexpected error occurred", "UNEXPECTED_ERROR");
            }
        }


        // Генерирует и возвращает access JWT
        private (string token, int expiresInSeconds) GenerateJwtWithTtl(User user)
        {
            var keyBytes = Convert.FromBase64String(_config["Jwt:Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var minutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 30;  // в appsettings-Secrets поставил 30
            var expires = now.AddMinutes(minutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,  user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),

                // дублируем для ASP.NET Core, чтобы User.Identity работал «из коробки»
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),

                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),     // Jti -> id токена. JWT описан в RFC 7519
                new Claim(ClaimTypes.Name,              user.Username),
                new Claim(ClaimTypes.Role,              user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: now,      // nbf
                expires: expires,  // exp
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            var expiresInSeconds = (int)TimeSpan.FromMinutes(minutes).TotalSeconds;
            return (jwt, expiresInSeconds);
        }

        private static string GenerateSecureToken()
        {
            // base64url (без + / =)
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

    }
}
