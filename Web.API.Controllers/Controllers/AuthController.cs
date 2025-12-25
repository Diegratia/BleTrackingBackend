using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers.Controllers
{
    //test
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
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var response = await _authService.LoginAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "Login successful",
                    collection = new { data = response },
                    code = 200
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    msg = ex.Message ?? "Invalid credentials",
                    collection = new { data = (object)null },
                    code = 401
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            await _authService.LogoutAsync(dto.RefreshToken);

            return Ok(new
            {
                success = true,
                msg = "Logout successful",
                collection = new { data = (object)null },
                code = 200
            });
        }


        

        [AllowAnonymous]
        [HttpPost("public/login/visitor")]
        public async Task<IActionResult> LoginVisitorAsync([FromBody] LoginVisitorDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var response = await _authService.LoginVisitorAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor Login successful",
                    collection = new { data = response },
                    code = 200
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    msg = ex.Message ?? "Invalid credentials",
                    collection = new { data = (object)null },
                    code = 401
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }


        [HttpPost("integration-login")]
        public async Task<IActionResult> IntegarationLoginAsync([FromBody] IntegrationLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var response = await _authService.IntegrationLoginAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "User Login successful",
                    collection = new { data = response },
                    code = 200
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    msg = ex.Message ?? "Invalid credentials",
                    collection = new { data = (object)null },
                    code = 401
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost("register")]
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var response = await _authService.RegisterAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Registration successful",
                    collection = new { data = response },
                    code = 201
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var response = await _authService.RefreshTokenAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "Refresh successful",
                    collection = new { data = response },
                    code = 200
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    msg = ex.Message ?? "Invalid credentials",
                    collection = new { data = (object)null },
                    code = 401
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("users")]
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _authService.GetAllUsersAsync();
                return Ok(new
                {
                    success = true,
                    msg = "User retrieved successfully",
                    collection = new { data = users },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("integration-users")]
        [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
        public async Task<IActionResult> GetAllIntegrationUsers()
        {
            try
            {
                var users = await _authService.GetAllIntegrationAsync();
                return Ok(new
                {
                    success = true,
                    msg = "User retrieved successfully",
                    collection = new { data = users },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("users/{id}")]
        [Authorize("RequireAuthenticatedUser")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var users = await _authService.GetUserByIdAsync(id);
                if (users == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "User not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "User group retrieved successfully",
                    collection = new { data = users },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

    [HttpPut("users/{id}")]
    [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
            return BadRequest(new
            {
                success = false,
                msg = "Validation failed: " + string.Join(", ", errors),
                collection = new { data = (object)null },
                code = 400
            });
        }

        try
        {
            await _authService.UpdateUserAsync(id, dto);
            return Ok(new
            {
                success = true,
                msg = "User updated successfully",
                collection = new { data = (object)null },
                code = 200
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                msg = ex.Message,
                collection = new { data = (object)null },
                code = 404
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                msg = $"Internal server error: {ex.Message}",
                collection = new { data = (object)null },
                code = 500
            });
        }
    }

    

    [HttpDelete("users/{id}")]
    [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _authService.DeleteUserAsync(id);
            return Ok(new
            {
                success = true,
                msg = "User deleted successfully",
                collection = new { data = (object)null },
                code = 200
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                msg = ex.Message,
                collection = new { data = (object)null },
                code = 404
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                msg = $"Internal server error: {ex.Message}",
                collection = new { data = (object)null },
                code = 500
            });
        }
    }


        [HttpPost("confirm-email")]
            [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                await _authService.ConfirmEmailAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "Email confirmed successfully, please set your password",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
        }

        // [HttpGet("confirm-visitor-invitation")]
        // [AllowAnonymous]
        //     public async Task<IActionResult> ConfirmVisitorInvitation(string email, string token)
        // {
        //     await _authService.ConfirmVisitorInvitationAsync(email);
        //     return Ok("Invitation confirmed successfully");
        // }

        [HttpPost("set-password")]
        [AllowAnonymous]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                await _authService.SetPasswordAsync(dto);
                return Ok(new
                {
                    success = true,
                    msg = "Password set successfully, you can now login",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
        }
    }
}


