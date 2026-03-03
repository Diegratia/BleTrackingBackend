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
    public class MstIntegrationController : ControllerBase
    {
        private readonly IMstIntegrationService _mstIntegrationService;

        public MstIntegrationController(IMstIntegrationService mstIntegrationService)
        {
            _mstIntegrationService = mstIntegrationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var integrations = await _mstIntegrationService.GetAllAsync();
            return Ok(ApiResponse.Success("Integrations retrieved successfully", integrations));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var integration = await _mstIntegrationService.GetByIdAsync(id);
            if (integration == null)
                return NotFound(ApiResponse.NotFound("Integration not found"));
            return Ok(ApiResponse.Success("Integration retrieved successfully", integration));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstIntegrationCreateDto createDto)
        {
            var createdIntegration = await _mstIntegrationService.CreateAsync(createDto);
            return StatusCode(201, ApiResponse.Created("Integration created successfully", createdIntegration));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstIntegrationUpdateDto updateDto)
        {
            await _mstIntegrationService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse.NoContent("Integration updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstIntegrationService.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Integration deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstIntegrationFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstIntegrationFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstIntegrationFilter();
            }

            var result = await _mstIntegrationService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Integrations filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstIntegrationService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstIntegration_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstIntegrationService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstIntegration_Report.xlsx");
        }
    }
}
