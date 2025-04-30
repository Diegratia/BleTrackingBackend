    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Repositories.DbContexts;
    using Entities.Models;
    using Data.ViewModels;
   
    using BCrypt.Net;

 namespace BusinessLogic.Services.Interface
    {
            public interface IAuthService
        {
            Task<AuthResponseDto> LoginAsync(LoginDto dto);
            Task<AuthResponseDto> RegisterAsync(RegisterDto dto); // Tambahkan ini
        }



    public class AuthService : IAuthService
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor; // Tambahkan untuk akses token

        public AuthService(
            BleTrackingDbContext context, 
            IMapper mapper, 
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new Exception("Invalid email or password.");
            if (user.StatusActive != StatusActive.Active)
                throw new Exception("Account is not active.");
            if (user.IsEmailConfirmation == 0)
                throw new Exception("Email not confirmed.");

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("Email is already registered.");

            // ambil info dari token 
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var currentUser = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found.");

            // cek role
            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary)
            {
                // jika role primary cuma bisa pilih role usercreated
                var targetGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(g => g.Id == dto.GroupId);
                if (targetGroup == null || targetGroup.LevelPriority != LevelPriority.UserCreated)
                    throw new UnauthorizedAccessException("Users with Primary role can only create accounts with UserCreated role.");
            }

            // buat akun
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsCreatedPassword = 1,
                IsEmailConfirmation = 0, // Harus konfirmasi email terlebih dahulu
                EmailConfirmationCode = Guid.NewGuid().ToString(),
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                StatusActive = StatusActive.Active,
                GroupId = dto.GroupId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // load group
    newUser.Group = await _context.UserGroups.FirstOrDefaultAsync(g => g.Id == newUser.GroupId);
    if (newUser.Group == null)
        throw new Exception("Assigned group not found.");

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
                new Claim(ClaimTypes.Role, user.Group.LevelPriority.ToString()) // Tambahkan role ke token
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
