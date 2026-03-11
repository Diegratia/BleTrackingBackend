using System;
// using Microsoft.AspNetCore.Identity

namespace Data.ViewModels
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

        public class LoginVisitorDto
    {
        public string EmailConfirmationCode { get; set; }
    }
        public class IntegrationLoginDto
    {
        public string IntegrationUsername { get; set; }
        public string ApiKeyValue { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsIntegration { get; set; }
        public Guid GroupId { get; set; }
        public Guid ApplicationId { get; set; }
        public string LevelPriority { get; set; }
        public bool IsHead { get; set; }
        public int IsEmailConfirmed { get; set; }
        public int Status { get; set; }
    }

        public class LogoutRequestDto
    {
        public string? RefreshToken { get; set; }
    }


    // public class RegisterDto
    // {
    //     public string Username { get; set; }
    //     public string Email { get; set; }
    //     public string Password { get; set; }
    //     public Guid GroupId { get; set; }
    // }

    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanApprovePatrol { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanAlarmAction { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanCreateMonitoringConfig { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanUpdateMonitoringConfig { get; set; }
    }

    public class ConfirmEmailDto
    {
        public string Email { get; set; }
        public string ConfirmationCode { get; set; }
    }

    public class SetPasswordDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class UserGroupDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public Guid ApplicationId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Status { get; set; }
    }

    public class CreateUserGroupDto
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Role modifier: true = Head Security (bisa action + approval), false = Operator Biasa (view only)
        /// </summary>
        public bool IsHead { get; set; } = false;
    }

    public class UpdateUserGroupDto
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Role modifier: true = Head Security (bisa action + approval), false = Operator Biasa (view only)
        /// </summary>
        public bool? IsHead { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }
        public int IsEmailConfirmation { get; set; }
        public bool isIntegration { get; set; }
        public string EmailConfirmationCode { get; set; }
        public DateTime EmailConfirmationExpiredAt { get; set; }
        public DateTime EmailConfirmationAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public Guid ApplicationId { get; set; }
        public int? StatusActive { get; set; }
    }

    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string? Password { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanApprovePatrol { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanAlarmAction { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanCreateMonitoringConfig { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanUpdateMonitoringConfig { get; set; }
    }

    /// <summary>
    /// DTO for direct user creation with password (no email verification required)
    /// </summary>
    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }  // Required for direct creation
        public Guid GroupId { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanApprovePatrol { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanAlarmAction { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanCreateMonitoringConfig { get; set; }

        /// <summary>
        /// Permission override: null = inherit dari Group.IsHead, true/false = override
        /// </summary>
        public bool? CanUpdateMonitoringConfig { get; set; }
    }

    /// <summary>
    /// DTO for forgot password request
    /// </summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// DTO for reset password with token
    /// </summary>
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string ResetToken { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO for changing password while logged in
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO for confirming email and setting password in one operation (URL-based flow)
    /// </summary>
    public class ConfirmAndSetPasswordDto
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

}


