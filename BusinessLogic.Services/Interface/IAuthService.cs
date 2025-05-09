using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Repositories.Repository;
using Entities.Models;
using Data.ViewModels;
using BCrypt.Net;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /*************  ✨ Windsurf Command ⭐  *************/
        /// <summary>
        /// Constructor for AuthService.
        /// </summary>
        /// <param name="userRepository">Repository for user operations.</param>
        /// <param name="userGroupRepository">Repository for user group operations.</param>
        /// <param name="mapper">Mapper for mapping between models and view models.</param>
        /// <param name="configuration">Configuration for the application.</param>
        /// <param name="httpContextAccessor">Accessor for getting the current HTTP context.</param>
        /*******  bf336e64-480f-4912-bf6f-facdb2c87530  *******/
        public AuthService(
            UserRepository userRepository,
            UserGroupRepository userGroupRepository,
            IMapper mapper,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new Exception("Invalid email or password.");
            if (user.StatusActive != StatusActive.Active)
                throw new Exception("Account is not active.");
            if (user.IsEmailConfirmation == 0)
                throw new Exception("Email not confirmed.");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var token = GenerateJwtToken(user);
            return new AuthResponseDto
            {
                Token = token,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                GroupId = user.GroupId,
                IsEmailConfirmed = user.IsEmailConfirmation == 1,
                StatusActive = user.StatusActive.ToString()
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Cek apakah email sudah ada
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new Exception("Email is already registered.");

            // Ambil info dari token
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found.");

            // Cek role
            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated);
            }

            // Buat akun
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsCreatedPassword = 1,
                IsEmailConfirmation = 0,
                EmailConfirmationCode = Guid.NewGuid().ToString(),
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                StatusActive = StatusActive.Active,
                GroupId = dto.GroupId
            };

            await _userRepository.AddAsync(newUser);

            var token = GenerateJwtToken(newUser);
            return new AuthResponseDto
            {
                Token = token,
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                GroupId = newUser.GroupId,
                IsEmailConfirmed = newUser.IsEmailConfirmation == 1,
                StatusActive = newUser.StatusActive.ToString()
            };
        }

        private string GenerateJwtToken(User user)
        {
            Console.WriteLine($"AuthService Jwt:Issuer = {_configuration["Jwt:Issuer"]}");
            Console.WriteLine($"AuthService Jwt:Audience = {_configuration["Jwt:Audience"]}");
            Console.WriteLine($"AuthService Jwt:Key = {_configuration["Jwt:Key"]}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("groupId", user.GroupId.ToString()),
                new Claim(ClaimTypes.Role, user.Group.LevelPriority.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine($"Generated token: {tokenString}");
            Console.WriteLine($"Token length: {tokenString.Length}");
            Console.WriteLine($"Token parts: {tokenString.Split('.').Length}");

            return tokenString;
        }
    }
}