using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Shared.Contracts;
using BusinessLogic.Services.Extension.RootExtension;
using System.Text.Json;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MstBuildingController : ControllerBase
    {
        private readonly IMstBuildingService _service;

        public MstBuildingController(IMstBuildingService service)
        {
            _service = service;
        }

        [HttpGet]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var buildings = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Buildings retrieved successfully", buildings));
        }
        [MinLevel(LevelPriority.PrimaryAdmin)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var building = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Building retrieved successfully", building));
        }
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstBuildingCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdBuilding = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Building created successfully", createdBuilding));
        }
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstBuildingUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var updatedBuilding = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Building updated successfully", updatedBuilding));
        }
        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Building deleted successfully"));
        }

        [HttpPost("filter")]
        [MinLevel(LevelPriority.PrimaryAdmin)]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));

            var filter = new MstBuildingFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstBuildingFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstBuildingFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Buildings filtered successfully", result));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var buildings = await _service.ImportAsync(file);
            return Ok(ApiResponse.Success("Buildings imported successfully", buildings));
        }
        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _service.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstBuilding_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _service.ExportExcelAsync();
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MstBuilding_Report.xlsx");
        }
        
        [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> OpenGetAll()
        {
            var buildings = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Buildings retrieved successfully", buildings));
        }
    }
}