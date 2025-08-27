using AutoMapper;
using Microsoft.EntityFrameworkCore;
using rozetochka_api.Application.Users.DTOs;
using rozetochka_api.Application.Users.Repository;
using rozetochka_api.Domain.Entities;
using rozetochka_api.Infrastructure.Identity.Interfaces;
using rozetochka_api.Shared;

namespace rozetochka_api.Application.Users.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
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



    }
}
