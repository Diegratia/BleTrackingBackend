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
    [MinLevel(LevelPriority.SuperAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class MstApplicationController : ControllerBase
    {
        private readonly IMstApplicationService _mstApplicationService;

        public MstApplicationController(IMstApplicationService mstApplicationService)
        {
            _mstApplicationService = mstApplicationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var applications = await _mstApplicationService.GetAllApplicationsAsync();
            return Ok(ApiResponse.Success("Applications retrieved successfully", applications));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var application = await _mstApplicationService.GetApplicationByIdAsync(id);
            if (application == null)
                return NotFound(ApiResponse.NotFound("Application not found"));
            return Ok(ApiResponse.Success("Application retrieved successfully", application));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstApplicationCreateDto createDto)
        {
            var createdApplication = await _mstApplicationService.CreateApplicationAsync(createDto);
            return StatusCode(201, ApiResponse.Created("Application created successfully", createdApplication));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstApplicationUpdateDto updateDto)
        {
            await _mstApplicationService.UpdateApplicationAsync(id, updateDto);
            return Ok(ApiResponse.NoContent("Application updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstApplicationService.DeleteApplicationAsync(id);
            return Ok(ApiResponse.NoContent("Application marked as deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstApplicationFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstApplicationFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstApplicationFilter();
            }

            var result = await _mstApplicationService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Applications filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstApplicationService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstApplication_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstApplicationService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstApplication_Report.xlsx");
        }
    }
}
