using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using Microsoft.AspNetCore.Authentication.Negotiate;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var response = await _authService.LoginAsync(dto);
            return Ok(ApiResponse.Success("Login successful", response));
        }

        [HttpGet("login-sso")]
        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        public async Task<IActionResult> LoginSso()
        {
            var windowsUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(windowsUsername))
                return Unauthorized(ApiResponse.Unauthorized("Windows Identity is missing. Browser may not have passed credentials."));

            try 
            {
                var response = await _authService.LoginSsoAsync(windowsUsername);
                return Ok(ApiResponse.Success("SSO Login successful", response));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.BadRequest(ex.Message));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken);
            return Ok(ApiResponse.Success("Logout successful"));
        }

        [AllowAnonymous]
        [HttpPost("public/login/visitor")]
        public async Task<IActionResult> LoginVisitorAsync([FromBody] LoginVisitorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
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
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
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
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
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
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
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
    }
}
