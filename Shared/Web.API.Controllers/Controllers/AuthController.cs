using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using Microsoft.AspNetCore.Authentication.Negotiate;
using System.Diagnostics;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IFeatureService _featureService;

        public AuthController(IAuthService authService, IFeatureService featureService)
        {
            _authService = authService;
            _featureService = featureService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.LoginAsync(dto);
            return Ok(ApiResponse.Success("Login successful", response));
        }

        /// <summary>
        /// Windows SSO Login endpoint
        /// Requires module.sso feature to be enabled
        /// </summary>
        [HttpGet("login-sso")]
        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        public async Task<IActionResult> LoginSso()
        {
            // Check if SSO feature is enabled
            var isSsoEnabled = _featureService.IsFeatureEnabled(Shared.BusinessLogic.Services.Feature.FeatureDefinition.ModuleSso);

            if (!isSsoEnabled)
            {
                return Unauthorized(ApiResponse.Forbidden("Single Sign-On (SSO) module is not enabled for this application"));
            }

            var windowsUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(windowsUsername))
                return Unauthorized(ApiResponse.Unauthorized("Windows Identity is missing. Browser may not have passed credentials."));

                var response = await _authService.LoginSsoAsync(windowsUsername);
                return Ok(ApiResponse.Success("SSO Login successful", response));
        }

        /// <summary>
        /// Check if SSO is enabled for the current application
        /// </summary>
        [HttpGet("sso/enabled")]
        [Authorize]
        public async Task<IActionResult> IsSsoEnabled()
        {
            var applicationIdClaim = User.FindFirst("ApplicationId")?.Value;
            if (string.IsNullOrEmpty(applicationIdClaim) || !Guid.TryParse(applicationIdClaim, out var applicationId))
            {
                return Ok(ApiResponse.Success("SSO status retrieved", new
                {
                    isEnabled = false,
                    message = "Application ID not found"
                }));
            }

            var isSsoEnabled = _featureService.IsFeatureEnabled(Shared.BusinessLogic.Services.Feature.FeatureDefinition.ModuleSso);

            return Ok(ApiResponse.Success("SSO status retrieved", new
            {
                isEnabled = isSsoEnabled,
                featureKey = "module.sso",
                featureName = "Single Sign-On (SSO)"
            }));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken );
            return Ok(ApiResponse.Success("Logout successful"));
        }

        [AllowAnonymous]
        [HttpPost("public/login/visitor")]
        public async Task<IActionResult> LoginVisitorAsync([FromBody] LoginVisitorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.LoginVisitorAsync(dto);
            return Ok(ApiResponse.Success("Visitor Login successful", response));
        }

        [HttpPost("integration-login")]
        public async Task<IActionResult> IntegarationLoginAsync([FromBody] IntegrationLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.IntegrationLoginAsync(dto);
            return Ok(ApiResponse.Success("User Login successful", response));
        }

        [HttpPost("register")]
        [MinLevel(LevelPriority.SuperAdmin)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.RegisterAsync(dto);
            return StatusCode(201, ApiResponse.Created("Registration successful", response));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.RefreshTokenAsync(dto);
            return Ok(ApiResponse.Success("Refresh successful", response));
        }

        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _authService.ConfirmEmailAsync(dto);
            return Ok(ApiResponse.Success("Email confirmed successfully, please set your password"));
        }

        [HttpPost("set-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _authService.SetPasswordAsync(dto);
            return Ok(ApiResponse.Success("Password set successfully, you can now login"));
        }

        [HttpPost("confirm-account")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmAndSetPassword([FromBody] ConfirmAndSetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _authService.ConfirmAndSetPasswordAsync(dto);
            return Ok(ApiResponse.Success("Email confirmed and password set successfully, you can now login"));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _authService.ForgotPasswordAsync(dto);
            return Ok(ApiResponse.Success("If an account with this email exists, a password reset link has been sent"));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _authService.ResetPasswordAsync(dto);
            return Ok(ApiResponse.Success("Password reset successfully, you can now login"));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Unauthorized("User not authenticated"));

            await _authService.ChangePasswordAsync(dto, Guid.Parse(userId));
            return Ok(ApiResponse.Success("Password changed successfully"));
        }
    }
}
