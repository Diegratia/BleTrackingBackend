// File: Web.API.Controllers/Controllers/Analytics/TrackingReportPresetController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using System;
using System.Threading.Tasks;
using Repositories.Repository.RepoModel;
using BusinessLogic.Services.Implementation.Analytics;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/tracking-presets")]
    [Authorize("RequireAllAndUserCreated")]
    public class TrackingReportPresetController : ControllerBase
    {
        private readonly ITrackingReportPresetService _presetService;
        private readonly ITrackingAnalyticsV2Service _analyticsV2Service;

        public TrackingReportPresetController(
            ITrackingReportPresetService presetService,
            ITrackingAnalyticsV2Service analyticsV2Service)
        {
            _presetService = presetService;
            _analyticsV2Service = analyticsV2Service;
        }

        // ==============================================
        // 1. APPLY PRESET - Get visitor session data
        // ==============================================
        [HttpPost("apply/{presetId}")]
        public async Task<IActionResult> ApplyPresetForVisitorSession(
        Guid presetId,
        [FromQuery] string? timezone)
        {
                    if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            try

            {
                var result = await _analyticsV2Service.GetVisitorSessionSummaryByPresetAsync(presetId, timezone);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==============================================
        // 2. GET ALL PRESETS
        // ==============================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var presets = await _presetService.GetAllAsync();
                return Ok(ApiResponse.Success("Presets retrieved successfully", presets));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        // ==============================================
        // 3. GET BY ID
        // ==============================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var preset = await _presetService.GetByIdAsync(id);
                if (preset == null)
                    return NotFound(ApiResponse.NotFound("Preset not found"));

                return Ok(ApiResponse.Success("Preset retrieved successfully", preset));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

          

        // ==============================================
        // 4. SAVE PRESET
        // ==============================================
        [HttpPost]
        public async Task<IActionResult> SavePreset([FromBody] CreateCustomPresetRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
                }

                var preset = await _presetService.SavePresetAsync(request);
                return Ok(ApiResponse.Success("Preset saved successfully", preset));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==============================================
        // 5. UPDATE PRESET
        // ==============================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePreset(Guid id, [FromBody] UpdatePresetRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
                }

                var preset = await _presetService.UpdateAsync(id, request);
                return Ok(ApiResponse.Success("Preset updated successfully", request));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse.NotFound("Preset not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==============================================
        // 6. DELETE PRESET
        // ==============================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreset(Guid id)
        {
            try
            {
                var success = await _presetService.DeleteAsync(id);
                if (!success)
                    return NotFound(ApiResponse.NotFound("Preset not found"));

                return Ok(ApiResponse.NoContent("Building deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate Excel: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // ==============================================
        // 7. SEED SYSTEM PRESETS (ADMIN ONLY)
        // ==============================================
        // [HttpPost("seed-system")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> SeedSystemPresets()
        // {
        //     try
        //     {
        //         await _presetService.SeedSystemPresetsAsync();
        //         return Ok(new { message = "System presets seeded successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { error = ex.Message });
        //     }
        // }
    }
}