using System;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]
    public class AlarmCategorySettingsController : ControllerBase
    {
        private readonly IAlarmCategorySettingsService _alarmCategorySettingsService;

        public AlarmCategorySettingsController(IAlarmCategorySettingsService alarmCategorySettingsService)
        {
            _alarmCategorySettingsService = alarmCategorySettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _alarmCategorySettingsService.GetAllAsync();
            return Ok(ApiResponse.Success("Alarm Category retrieved successfully", categories));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _alarmCategorySettingsService.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse.NotFound("Alarm Category not found"));
            return Ok(ApiResponse.Success("Alarm Category retrieved successfully", category));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlarmCategorySettingsUpdateDto dto)
        {
            await _alarmCategorySettingsService.UpdateAsync(id, dto);
            return Ok(ApiResponse.NoContent("Alarm Category updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _alarmCategorySettingsService.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Alarm Category deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new AlarmCategorySettingsFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<AlarmCategorySettingsFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AlarmCategorySettingsFilter();
            }

            var result = await _alarmCategorySettingsService.FilterAsync(request, filter);
            return Ok(ApiResponse.Success("Alarm Categories filtered successfully", result));
        }
    }
}
