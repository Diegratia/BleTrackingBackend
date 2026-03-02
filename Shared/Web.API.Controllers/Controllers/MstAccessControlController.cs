using System;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using DataView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class MstAccessControlController : ControllerBase
    {
        private readonly IMstAccessControlService _mstAccessControlService;

        public MstAccessControlController(IMstAccessControlService mstAccessControlService)
        {
            _mstAccessControlService = mstAccessControlService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accessControls = await _mstAccessControlService.GetAllAsync();
            return Ok(ApiResponse.Success("Access Controls retrieved successfully", accessControls));
        }

        [HttpGet("unassigned")]
        public async Task<IActionResult> GetAllUnassignedAsync()
        {
            var accessControls = await _mstAccessControlService.GetAllUnassignedAsync();
            return Ok(ApiResponse.Success("Unassigned Access Controls retrieved successfully", accessControls));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var accessControl = await _mstAccessControlService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Access Control retrieved successfully", accessControl));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstAccessControlCreateDto mstAccessControlDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdAccessControl = await _mstAccessControlService.CreateAsync(mstAccessControlDto);
            return StatusCode(201, ApiResponse.Created("Access Control created successfully", createdAccessControl));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstAccessControlUpdateDto mstAccessControlDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _mstAccessControlService.UpdateAsync(id, mstAccessControlDto);
            return Ok(ApiResponse.Success("Access Control updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstAccessControlService.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Access Control deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstAccessControlFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstAccessControlFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstAccessControlFilter();
            }

            var result = await _mstAccessControlService.FilterAsync(request, filter);
            return Ok(ApiResponse.Success("Access Control filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstAccessControlService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstAccessControl_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstAccessControlService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstAccessControl_Report.xlsx");
        }

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var accessControls = await _mstAccessControlService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Access Controls retrieved successfully", accessControls));
        }

        [HttpPost("open/filter")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstAccessControlFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstAccessControlFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstAccessControlFilter();
            }

            var result = await _mstAccessControlService.FilterAsync(request, filter);
            return Ok(ApiResponse.Success("Access Control filtered successfully", result));
        }
    }
}
