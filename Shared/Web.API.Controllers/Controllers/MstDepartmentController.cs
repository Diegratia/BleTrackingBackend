using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]
    public class MstDepartmentController : ControllerBase
    {
        private readonly IMstDepartmentService _mstDepartmentService;

        public MstDepartmentController(IMstDepartmentService mstDepartmentService)
        {
            _mstDepartmentService = mstDepartmentService;
        }

        // GET: api/MstDepartment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _mstDepartmentService.GetAllAsync();
            return Ok(ApiResponse.Success("Departments retrieved successfully", departments));
        }

        // GET: api/MstDepartment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _mstDepartmentService.GetByIdAsync(id);
            if (department == null) return NotFound(ApiResponse.NotFound("Department not found"));
            return Ok(ApiResponse.Success("Department retrieved successfully", department));
        }

        // POST: api/MstDepartment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstDepartmentCreateDto mstDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdDepartment = await _mstDepartmentService.CreateAsync(mstDepartmentDto);
            return StatusCode(201, ApiResponse.Created("Department created successfully", createdDepartment));
        }

        // PUT: api/MstDepartment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstDepartmentUpdateDto mstDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstDepartmentService.UpdateAsync(id, mstDepartmentDto);
            return Ok(ApiResponse.Success("Department updated successfully"));
        }

        // DELETE: api/MstDepartment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstDepartmentService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Department marked as deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new MstDepartmentFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstDepartmentFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstDepartmentFilter();
            }

            var result = await _mstDepartmentService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Departments filtered successfully", result));
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var departments = await _mstDepartmentService.ImportAsync(file);
            return Ok(ApiResponse.Success("Departments imported successfully", departments));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstDepartmentService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstDepartment_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstDepartmentService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstDepartment_Report.xlsx");
        }

        // OPEN APIs

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var departments = await _mstDepartmentService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Departments retrieved successfully", departments));
        }

        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var department = await _mstDepartmentService.GetByIdAsync(id);
            if (department == null) return NotFound(ApiResponse.NotFound("Department not found"));
            return Ok(ApiResponse.Success("Department retrieved successfully", department));
        }

        [AllowAnonymous]
        [HttpPost("open/create")]
        public async Task<IActionResult> OpenCreate([FromBody] MstDepartmentCreateDto mstDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdDepartment = await _mstDepartmentService.CreateAsync(mstDepartmentDto);
            return StatusCode(201, ApiResponse.Created("Department created successfully", createdDepartment));
        }

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstDepartmentUpdateDto mstDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstDepartmentService.UpdateAsync(id, mstDepartmentDto);
            return Ok(ApiResponse.Success("Department updated successfully"));
        }

        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstDepartmentService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Department marked as deleted successfully"));
        }

        [AllowAnonymous]
        [HttpPost("open/{filter}")]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new MstDepartmentFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstDepartmentFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstDepartmentFilter();
            }

            var result = await _mstDepartmentService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Departments filtered successfully", result));
        }
    }
}