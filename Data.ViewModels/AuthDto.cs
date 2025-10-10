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
        public int IsEmailConfirmed { get; set; }
        public string StatusActive { get; set; }
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
    }

    public class UpdateUserGroupDto
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public Guid ApplicationId { get; set; }
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
        public string Password { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }
    }
    
}


