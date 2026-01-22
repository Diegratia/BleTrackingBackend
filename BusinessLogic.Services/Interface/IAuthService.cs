




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
using System.Security.Cryptography;
using BusinessLogic.Services.Implementation;
using Bogus.DataSets;
using Helpers.Consumer;
using Helpers.Consumer.Mqtt;

namespace BusinessLogic.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task ConfirmEmailAsync(ConfirmEmailDto dto);
        Task SetPasswordAsync(SetPasswordDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetAllIntegrationAsync();
        Task<UserDto> GetUserByIdAsync(Guid id);
         Task LogoutAsync(string refreshToken);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
        Task DeleteUserAsync(Guid id);
        Task<UserGroupDto> CreateGroupAsync(CreateUserGroupDto dto);
        Task<UserGroupDto> UpdateGroupAsync(Guid id, UpdateUserGroupDto dto);
        Task DeleteGroupAsync(Guid id);
        Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync();
        Task<UserGroupDto> GetGroupByIdAsync(Guid id);
        Task<AuthResponseDto> LoginVisitorAsync(LoginVisitorDto dto);
        Task<AuthResponseDto> IntegrationLoginAsync(IntegrationLoginDto dto);

        
            // Task ConfirmVisitorInvitationAsync(string email); 
    }
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly MstIntegrationRepository _mstIntegrationRepository;
        // private readonly VisitorRepository _visitorRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IAuditEmitter _audit;



        public AuthService(
            UserRepository userRepository,
            UserGroupRepository userGroupRepository,
            RefreshTokenRepository refreshTokenRepository,
            MstIntegrationRepository mstIntegrationRepository,
            IMapper mapper,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _mstIntegrationRepository = mstIntegrationRepository;
            _mapper = mapper;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _audit = audit;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username.ToLower());
            // var user = await _userRepository.GetByEmailAsync(dto.Email.ToLower());
            if (user == null || string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                Console.WriteLine("Invalid username or password");
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            if (user.StatusActive != StatusActive.Active)
            {
                Console.WriteLine("Account is not active");
                throw new UnauthorizedAccessException("Account is not active");
            }
            if (user.IsEmailConfirmation == 0)
            {
                Console.WriteLine("Email not confirmed");
                throw new UnauthorizedAccessException("Email not confirmed");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.SaveRefreshTokenAsync(refreshTokenEntity);

            await _audit.Action(
                AuditEmitter.AuditAction.LOGIN,
                "User",
                "User login successfully",
                new {
                    username = user.Username,
                    ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
                }
            );

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                GroupId = user.GroupId,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString(),
                IsEmailConfirmed = user.IsEmailConfirmation,
                StatusActive = user.StatusActive.ToString()
            };
        }
        
        public async Task<AuthResponseDto> LoginVisitorAsync(LoginVisitorDto dto)
        {
            var user = await _userRepository.GetByConfirmationCodeAsync(dto.EmailConfirmationCode.ToLower());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid Confirmation Code");
            if (user.Group.LevelPriority != LevelPriority.UserCreated && user.Group.LevelPriority != LevelPriority.Secondary)
                throw new UnauthorizedAccessException("Only Usercreated or secondary can login");
            if (user.EmailConfirmationExpiredAt < DateTime.UtcNow)
                throw new Exception("Invitation code expired");
                
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.SaveRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                GroupId = user.GroupId,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString(),
                IsEmailConfirmed = user.IsEmailConfirmation,
                StatusActive = user.StatusActive.ToString()
            };
        }
        
        public async Task<AuthResponseDto> IntegrationLoginAsync(IntegrationLoginDto dto)
        {
            
            var integration = await _mstIntegrationRepository.GetApiKeyAsync(dto.ApiKeyValue);
            if (integration == null)
                throw new UnauthorizedAccessException("Invalid API Key");

            // Cari pengguna dengan Username dan ApplicationId
            var user = await _userRepository.GetByIntegrationUsername(dto.IntegrationUsername.ToLower());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid Username");

            // Validasi status
            if (user.StatusActive != StatusActive.Active)
                throw new UnauthorizedAccessException("Account is not active");
            if (user.IsEmailConfirmation == 0)
                throw new UnauthorizedAccessException("Email not confirmed");

            // Validasi level akses (fleksibel)
            if (user.Group.LevelPriority != LevelPriority.SuperAdmin && user.Group.LevelPriority != LevelPriority.PrimaryAdmin)
                throw new UnauthorizedAccessException("Only SuperAdmin or PrimaryAdmin can login via integration");

            // Perbarui LastLoginAt
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            // Buat token
            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.SaveRefreshTokenAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsIntegration = user.IsIntegration,
                GroupId = user.GroupId,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString(),
                IsEmailConfirmed = user.IsEmailConfirmation,
                StatusActive = user.StatusActive.ToString()
            };
        }

        //     public async Task<AuthResponseDto> IntegrationLoginAsync(IntegrationLoginDto dto)
        // {
        //     var user = await _userRepository.GetByIntegrationUsername(dto.IntegrationUsername.ToLower());
        //     if (user == null)
        //         throw new UnauthorizedAccessException("Invalid Username");
        //     if (user.Group.LevelPriority != LevelPriority.SuperAdmin)
        //         throw new UnauthorizedAccessException("Only Integration Level can login");

        //     user.LastLoginAt = DateTime.UtcNow;
        //     await _userRepository.UpdateAsync(user);

        //     var accessToken = GenerateJwtToken(user);
        //     var refreshToken = GenerateRefreshToken();

        //     var refreshTokenEntity = new RefreshToken
        //     {
        //         Id = Guid.NewGuid(),
        //         UserId = user.Id,
        //         Token = refreshToken,
        //         ExpiryDate = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpiryInDays")),
        //         CreatedAt = DateTime.UtcNow
        //     };

        //     await _refreshTokenRepository.SaveRefreshTokenAsync(refreshTokenEntity);

        //     return new AuthResponseDto
        //     {
        //         Token = accessToken,
        //         RefreshToken = refreshToken,
        //         Id = user.Id,
        //         Username = user.Username,
        //         Email = user.Email,
        //         IsIntegration = user.IsIntegration,
        //         GroupId = user.GroupId,
        //         ApplicationId = user.Group.ApplicationId,
        //         LevelPriority = user.Group.LevelPriority.ToString(),
        //         IsEmailConfirmed = user.IsEmailConfirmation,
        //         StatusActive = user.StatusActive.ToString()
        //     };
        // }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email.ToLower()))
                throw new Exception("Email is already registered");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            // if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            // {
            //     await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated);
            // }        

            if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated, LevelPriority.Primary, LevelPriority.PrimaryAdmin);
            }

            // var confirmationCode = Guid.NewGuid().ToString();
            var confirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                Password = null ?? "",
                IsCreatedPassword = 0,
                IsEmailConfirmation = 0,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.MinValue,
                StatusActive = StatusActive.NonActive,
                ApplicationId = (await _userGroupRepository.GetByIdAsync(dto.GroupId)).ApplicationId,
                GroupId = dto.GroupId
            };

            await _userRepository.AddAsync(newUser);

            // Kirim email konfirmasi
            await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationCode);

            return new AuthResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                GroupId = newUser.GroupId,
                ApplicationId = (await _userGroupRepository.GetByIdAsync(dto.GroupId)).ApplicationId,
                IsEmailConfirmed = 0,
                StatusActive = newUser.StatusActive.ToString()
            };
        }

        

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found for update");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole == LevelPriority.Primary || currentUserRole == LevelPriority.PrimaryAdmin)
            {
                await _userGroupRepository.ValidateGroupRoleAsync(dto.GroupId, LevelPriority.UserCreated, LevelPriority.Primary, LevelPriority.PrimaryAdmin);
            }

            user.Username = dto.Username.ToLower();
            user.Email = dto.Email.ToLower();
            user.GroupId = dto.GroupId;
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                user.IsCreatedPassword = 1;
            }

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.System && currentUserRole != LevelPriority.SuperAdmin && currentUserRole != LevelPriority.PrimaryAdmin)
            throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can delete user");

            await _userRepository.DeleteAsync(id);
        }

        public async Task ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var user = await _userRepository.GetByEmailConfirmPasswordAsyncRaw(dto.Email.ToLower());
            if (user == null)
                throw new Exception("23 User not found");
            if (user.IsEmailConfirmation == 1)
                throw new Exception("Email already confirmed");
            if (user.EmailConfirmationCode != dto.ConfirmationCode)
                throw new Exception("Invalid confirmation code");
            if (user.EmailConfirmationExpiredAt < DateTime.UtcNow)
                throw new Exception("Confirmation code expired");

            user.IsEmailConfirmation = 1;
            user.EmailConfirmationAt = DateTime.UtcNow;
            await _userRepository.UpdateConfirmAsync(user);
        }

        public async Task SetPasswordAsync(SetPasswordDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            var user = await _userRepository.GetByEmailSetPasswordAsync(dto.Email.ToLower());
            if (user == null)
                throw new Exception("User not found");
            if (user.IsEmailConfirmation == 0)
                throw new Exception("Email not confirmed");
            if (user.IsCreatedPassword == 1)
                throw new Exception("Password already set");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.IsCreatedPassword = 1;
            user.StatusActive = StatusActive.Active;
            await _userRepository.UpdateConfirmAsync(user);
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var tokenEntity = await _refreshTokenRepository.GetRefreshTokenAsync(dto.RefreshToken);
            if (tokenEntity == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
            if (user == null || user.StatusActive != StatusActive.Active)
                throw new UnauthorizedAccessException("User not found or inactive");

            var newAccessToken = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = newAccessToken,
                // RefreshToken = null ?? "", // Tidak mengganti refresh token
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                GroupId = user.GroupId,
                ApplicationId = user.Group.ApplicationId,
                IsEmailConfirmed = user.IsEmailConfirmation = 1,
                StatusActive = user.StatusActive.ToString()
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("groupId", user.GroupId.ToString()),
                new Claim("ApplicationId", user.Group.ApplicationId.ToString()),
                new Claim("groupName", user.Group.Name),
                new Claim(ClaimTypes.Role, user.Group.LevelPriority.ToString()),
                new Claim("level", ((int)user.Group.LevelPriority).ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddYears(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<IEnumerable<UserDto>> GetAllIntegrationAsync()
        {
            var users = await _userRepository.GetAllIntegrationAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserGroupDto> CreateGroupAsync(CreateUserGroupDto dto)
        {
        var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

        var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
        if (currentUser == null)
            throw new UnauthorizedAccessException("Current user not found");

        var currentUserRole = currentUser.Group?.LevelPriority;
        if (currentUserRole != LevelPriority.System && currentUserRole != LevelPriority.SuperAdmin && currentUserRole != LevelPriority.PrimaryAdmin)
            throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can create groups");

        if (!Enum.TryParse<LevelPriority>(dto.LevelPriority.ToString(), out var targetPriority))
        throw new ArgumentException("Invalid level priority");

        if ((int)targetPriority < (int)currentUserRole)
            throw new UnauthorizedAccessException("You can only assign roles equal to or lower than your own");



        var userGroup = _mapper.Map<UserGroup>(dto);
        userGroup.Id = Guid.NewGuid();
        userGroup.Status = 1;
        userGroup.CreatedAt = DateTime.UtcNow;
        userGroup.CreatedBy = username;
        userGroup.UpdatedAt = DateTime.UtcNow;
        userGroup.UpdatedBy = username;
        var createdGroup = await _userGroupRepository.AddAsync(userGroup);
        return _mapper.Map<UserGroupDto>(createdGroup);
    }
    


    public async Task<UserGroupDto> UpdateGroupAsync(Guid id, UpdateUserGroupDto dto)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.Primary && currentUserRole != LevelPriority.PrimaryAdmin && currentUserRole != LevelPriority.System)
                throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can Update groups");

                if (!Enum.TryParse<LevelPriority>(dto.LevelPriority.ToString(), out var targetPriority))
        throw new ArgumentException("Invalid level priority");

        if ((int)targetPriority < (int)currentUserRole)
            throw new UnauthorizedAccessException("You can only assign roles equal to or lower than your own");

            var userGroup = _mapper.Map<UserGroup>(dto);
            userGroup.Id = id;
            await _userGroupRepository.UpdateAsync(userGroup);
            return _mapper.Map<UserGroupDto>(userGroup);
        }
        
        public async Task LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return;
        }



    public async Task DeleteGroupAsync(Guid id)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var currentUserRole = currentUser.Group?.LevelPriority;
            if (currentUserRole != LevelPriority.Primary && currentUserRole != LevelPriority.PrimaryAdmin && currentUserRole != LevelPriority.System)
                throw new UnauthorizedAccessException("Only System, SuperAdmin, or PrimaryAdmin roles can Delete groups");

            await _userGroupRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync()
        {
            var groups = await _userGroupRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserGroupDto>>(groups);
        }

        public async Task<UserGroupDto> GetGroupByIdAsync(Guid id)
        {
            var group = await _userGroupRepository.GetByIdAsync(id);
            if (group == null)
                throw new KeyNotFoundException("User group not found");
            return _mapper.Map<UserGroupDto>(group);
        }
    }
}
