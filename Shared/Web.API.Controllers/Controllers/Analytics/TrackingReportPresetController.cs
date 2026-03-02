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
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers.Analytics
{
    [ApiController]
    [Route("api/tracking-presets")]
    [MinLevel(LevelPriority.Primary)]

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
        TrackingAnalyticsFilter overrideRequest)
        {
            var result = await _analyticsV2Service.GetVisitorSessionSummaryByPresetAsync(presetId, overrideRequest);
            return Ok(ApiResponse.Success("Preset applied successfully", result));
        }

        // ==============================================
        // 2. GET ALL PRESETS
        // ==============================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var presets = await _presetService.GetAllAsync();
            return Ok(ApiResponse.Success("Presets retrieved successfully", presets));
        }

        // ==============================================
        // 3. GET BY ID
        // ==============================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var preset = await _presetService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Preset retrieved successfully", preset));
        }

        // ==============================================
        // 4. SAVE PRESET
        // ==============================================
        [HttpPost]
        public async Task<IActionResult> SavePreset([FromBody] CreateCustomPresetRequest request)
        {
            var preset = await _presetService.SavePresetAsync(request);
            return Ok(ApiResponse.Success("Preset saved successfully", preset));
        }

        // ==============================================
        // 5. UPDATE PRESET
        // ==============================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePreset(Guid id, [FromBody] UpdatePresetRequest request)
        {
            var preset = await _presetService.UpdateAsync(id, request);
            return Ok(ApiResponse.Success("Preset updated successfully", preset));
        }

        // ==============================================
        // 6. DELETE PRESET
        // ==============================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePreset(Guid id)
        {
            await _presetService.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Preset deleted successfully"));
        }
    }
}
