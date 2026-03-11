




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
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Helpers.Consumer.Mqtt;
using System.Diagnostics;

namespace BusinessLogic.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task ConfirmEmailAsync(ConfirmEmailDto dto);
        Task SetPasswordAsync(SetPasswordDto dto);
        Task ConfirmAndSetPasswordAsync(ConfirmAndSetPasswordDto dto);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto);
         Task LogoutAsync(string refreshToken);
        Task<AuthResponseDto> LoginVisitorAsync(LoginVisitorDto dto);
        Task<AuthResponseDto> IntegrationLoginAsync(IntegrationLoginDto dto);
        Task<AuthResponseDto> LoginSsoAsync(string windowsUsername);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
        Task ChangePasswordAsync(ChangePasswordDto dto, Guid userId);
    }
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly UserGroupRepository _userGroupRepository;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly MstIntegrationRepository _mstIntegrationRepository;
        private readonly GroupBuildingAccessRepository _groupBuildingAccessRepository;
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
            GroupBuildingAccessRepository groupBuildingAccessRepository,
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
            _groupBuildingAccessRepository = groupBuildingAccessRepository;
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
                throw new UnauthorizedAccessException("Invalid username or password");
            }
            if (user.Status != 1)
            {
                throw new UnauthorizedAccessException("Account is not active");
            }
            if (user.IsEmailConfirmation == 0)
            {
                throw new UnauthorizedAccessException("Email not confirmed");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = await GenerateJwtToken(user);
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

             _audit.Action(
                AuditEmitter.AuditAction.LOGIN,
                "User",
                "User login successfully",
                new {
                    username = user.Username,
                    ip = _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString()
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
                IsHead = user.Group.IsHead,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString()!,
                IsEmailConfirmed = user.IsEmailConfirmation,
                Status = user.Status
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

            var accessToken = await GenerateJwtToken(user);
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
                IsHead = user.Group.IsHead,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString()!,
                IsEmailConfirmed = user.IsEmailConfirmation,
                Status = user.Status
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
            if (user.Status != 1)
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
            var accessToken = await GenerateJwtToken(user);
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
                IsHead = user.Group.IsHead,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString()!,
                IsEmailConfirmed = user.IsEmailConfirmation,
                Status = user.Status
            };
        }

        public async Task<AuthResponseDto> LoginSsoAsync(string windowsUsername)
        {
            if (string.IsNullOrWhiteSpace(windowsUsername))
                throw new UnauthorizedAccessException("Windows identity not provided");

            // Extract just the username if it's in the format DOMAIN\username or username@domain
            string username = windowsUsername;
            Console.WriteLine("ini username",username);
            if (username.Contains("\\"))
                username = username.Split('\\').Last();
            else if (username.Contains("@"))
                username = username.Split('@').First();

            var user = await _userRepository.GetByUsernameAsync(username.ToLower());
            Console.WriteLine("ini user dari db",user);
            if (user == null)
                throw new UnauthorizedAccessException("User not found or not registered in CMS");

            if (user.Status != 1)
                throw new UnauthorizedAccessException("Account is not active");
            if (user.IsEmailConfirmation == 0)
                throw new UnauthorizedAccessException("Email not confirmed");

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            var accessToken = await GenerateJwtToken(user);
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

            _audit.Action(
                AuditEmitter.AuditAction.LOGIN,
                "User",
                "User SSO login successfully",
                new {
                    username = user.Username,
                    windowsIdentity = windowsUsername,
                    ip = _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString()
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
                IsHead = user.Group.IsHead,
                ApplicationId = user.Group.ApplicationId,
                LevelPriority = user.Group.LevelPriority.ToString()!,
                IsEmailConfirmed = user.IsEmailConfirmation,
                Status = user.Status
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userRepository.EmailExistsAsync(dto.Email.ToLower()))
                throw new Exception("Email is already registered");

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
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

            // Generate secure confirmation token (full GUID instead of 6-character code)
            var confirmationCode = Guid.NewGuid().ToString("N"); // 32-character secure token

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                Password = "",
                IsCreatedPassword = 0,
                IsEmailConfirmation = 0,
                EmailConfirmationCode = confirmationCode,
                EmailConfirmationExpiredAt = DateTime.UtcNow.AddDays(7),
                EmailConfirmationAt = DateTime.UtcNow,
                LastLoginAt = DateTime.MinValue,
                Status = 0,
                ApplicationId = (await _userGroupRepository.GetByIdAsync(dto.GroupId)).ApplicationId,
                GroupId = dto.GroupId,
                // Permission flags dari DTO
                CanApprovePatrol = dto.CanApprovePatrol,
                CanAlarmAction = dto.CanAlarmAction,
                CanCreateMonitoringConfig = dto.CanCreateMonitoringConfig,
                CanUpdateMonitoringConfig = dto.CanUpdateMonitoringConfig
            };

            await _userRepository.AddAsync(newUser);

            // Build confirmation URL with frontend base URL from configuration
            var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "http://localhost:3000";
            var confirmationUrl = $"{frontendBaseUrl}/user-form?type=confirm&token={confirmationCode}&email={newUser.Email}";

            // Kirim email konfirmasi
            await _emailService.SendConfirmationEmailAsync(newUser.Email, newUser.Username, confirmationUrl);

            return new AuthResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                GroupId = newUser.GroupId,
                ApplicationId = (await _userGroupRepository.GetByIdAsync(dto.GroupId)).ApplicationId,
                IsEmailConfirmed = 0,
                Status = newUser.Status
            };
        }

        

        public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                throw new UnauthorizedAccessException("User not authenticated");

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
            if (currentUser == null)
                throw new UnauthorizedAccessException("Current user not found");

            var user = await _userRepository.GetByIdEntityAsync(id);
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

            var currentUser = await _userRepository.GetByIdEntityAsync(Guid.Parse(currentUserId));
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

            if (user.EmailConfirmationCode.Trim() != dto.ConfirmationCode.Trim().ToUpper())
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
            user.Status = 1;
            await _userRepository.UpdateConfirmAsync(user);
        }

        public async Task ConfirmAndSetPasswordAsync(ConfirmAndSetPasswordDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            // Use GetByEmailConfirmPasswordAsync (no status filter) to find user regardless of confirmation status
            var user = await _userRepository.GetByEmailConfirmPasswordAsync(dto.Email.ToLower());
            if (user == null)
                throw new Exception("User not found");
            if (user.IsEmailConfirmation == 1)
                throw new Exception("Email already confirmed");
            if (user.IsCreatedPassword == 1)
                throw new Exception("Password already set");

            // Validate confirmation token
            if (user.EmailConfirmationCode.Trim().ToUpper() != dto.Token.Trim().ToUpper())
                throw new Exception("Invalid confirmation token");
            if (user.EmailConfirmationExpiredAt < DateTime.UtcNow)
                throw new Exception("Confirmation token expired");

            // Confirm email and set password in one atomic operation
            user.IsEmailConfirmation = 1;
            user.EmailConfirmationAt = DateTime.UtcNow;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.IsCreatedPassword = 1;
            user.Status = 1;
            await _userRepository.UpdateConfirmAsync(user);
        }


        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var tokenEntity = await _refreshTokenRepository.GetRefreshTokenAsync(dto.RefreshToken);
            if (tokenEntity == null)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var user = await _userRepository.GetByIdEntityAsync(tokenEntity.UserId);
            if (user == null || user.Status != 1)
                throw new UnauthorizedAccessException("User not found or inactive");

            var newAccessToken = await GenerateJwtToken(user);

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
                Status = user.Status
            };
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("groupId", user.GroupId.ToString()),
                new Claim("ApplicationId", user.Group.ApplicationId.ToString()),
                new Claim("groupName", user.Group.Name!),
                new Claim(ClaimTypes.Role, user.Group.LevelPriority.ToString()!),
                new Claim("level", ((int)user.Group.LevelPriority!).ToString()),
                new Claim("isHead", user.Group.IsHead.ToString())
            };

            if (user.Group.LevelPriority != LevelPriority.System && user.Group.LevelPriority != LevelPriority.SuperAdmin)
            {
                var accessibleBuildingIds = await _groupBuildingAccessRepository.GetAccessibleBuildingIdsAsync(user.GroupId);
                var buildingIdsString = string.Join(",", accessibleBuildingIds.Select(id => id.ToString()));
                claims.Add(new Claim("accessibleBuildings", buildingIdsString));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
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
            return user == null ? null! : _mapper.Map<UserDto>(user);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _refreshTokenRepository.DeleteRefreshTokenAsync(refreshToken);
            }
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            // Find user by email (use Raw to avoid application filter for this operation)
            var user = await _userRepository.GetByEmailConfirmPasswordAsync(dto.Email.ToLower());

            // Always return success to prevent email enumeration
            if (user == null)
                return;

            var resetToken = Guid.NewGuid().ToString("N"); 

            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1); 

            await _userRepository.UpdatePasswordResetAsync(user);

            var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "http://localhost:3000";
            var resetUrl = $"{frontendBaseUrl}/user-form?type=reset&token={resetToken}&email={user.Email}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetUrl);

            _audit.Action(
                AuditEmitter.AuditAction.FORGOT_PASSWORD,
                user.Username,
                "Password reset requested",
                new { email = user.Email, username = user.Username }
            );
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ArgumentException("Passwords do not match");

            // Validate reset token
            var user = await _userRepository.GetByPasswordResetTokenAsync(dto.ResetToken);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid or expired reset token");

            if (user.Email.ToLower() != dto.Email.ToLower())
                throw new UnauthorizedAccessException("Email does not match reset token");

            // Hash new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsCreatedPassword = 1;
            user.PasswordResetToken = null; // Clear reset token
            user.PasswordResetTokenExpiresAt = null;

            await _userRepository.UpdatePasswordResetAsync(user);

            _audit.Action(
                AuditEmitter.AuditAction.RESET_PASSWORD,
                user.Username,
                "Password reset successfully",
                new { email = user.Email, username = user.Username }
            );
        }

        public async Task ChangePasswordAsync(ChangePasswordDto dto, Guid userId)
        {
            // Validate passwords match
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ArgumentException("New password and confirmation password do not match");

            // Get user
            var user = await _userRepository.GetByIdAsyncRaw(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            // Verify current password
            if (string.IsNullOrEmpty(user.Password) || !BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                throw new UnauthorizedAccessException("Current password is incorrect");

            // Hash new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsCreatedPassword = 1;

            await _userRepository.UpdateRawAsync(user);

            // Invalidate all refresh tokens for security
            await _refreshTokenRepository.DeleteAllByUserIdAsync(userId);

            _audit.Action(
                AuditEmitter.AuditAction.RESET_PASSWORD,
                user.Username,
                "Password changed successfully",
                new { email = user.Email, username = user.Username }
            );
        }
    }
}
