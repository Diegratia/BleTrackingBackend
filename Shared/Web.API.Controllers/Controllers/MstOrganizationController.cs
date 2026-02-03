using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Implementation;
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
    public class MstOrganizationController : ControllerBase
    {
        private readonly IMstOrganizationService _mstOrganizationService;

        public MstOrganizationController(IMstOrganizationService mstOrganizationService)
        {
            _mstOrganizationService = mstOrganizationService;
        }

        // GET: api/MstOrganization
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organizations = await _mstOrganizationService.GetAllOrganizationsAsync();
            return Ok(ApiResponse.Success("Organizations retrieved successfully", organizations));
        }

        // GET: api/MstOrganization/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var organization = await _mstOrganizationService.GetOrganizationByIdAsync(id);
            if (organization == null) return NotFound(ApiResponse.NotFound("Organization not found"));
            return Ok(ApiResponse.Success("Organization retrieved successfully", organization));
        }

        // POST: api/MstOrganization
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstOrganizationCreateDto mstOrganizationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdOrganization = await _mstOrganizationService.CreateOrganizationAsync(mstOrganizationDto);
            return StatusCode(201, ApiResponse.Created("Organization created successfully", createdOrganization));
        }

        // PUT: api/MstOrganization/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstOrganizationUpdateDto mstOrganizationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstOrganizationService.UpdateOrganizationAsync(id, mstOrganizationDto);
            return Ok(ApiResponse.Success("Organization updated successfully"));
        }

        // DELETE: api/MstOrganization/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstOrganizationService.DeleteOrganizationAsync(id);
            return Ok(ApiResponse.Success("Organization marked as deleted successfully"));
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

            var filter = new MstOrganizationFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstOrganizationFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstOrganizationFilter();
            }

            var result = await _mstOrganizationService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Organizations filtered successfully", result));
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var floors = await _mstOrganizationService.ImportAsync(file);
            return Ok(ApiResponse.Success("Organizations imported successfully", floors));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstOrganizationService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstOrganization_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstOrganizationService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstOrganization_Report.xlsx");
        }

        //OPEN APIs

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var organizations = await _mstOrganizationService.OpenGetAllOrganizationsAsync();
            return Ok(ApiResponse.Success("Organizations retrieved successfully", organizations));
        }

        // GET: api/MstOrganization/{id}
        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var organization = await _mstOrganizationService.GetOrganizationByIdAsync(id);
            if (organization == null) return NotFound(ApiResponse.NotFound("Organization not found"));
            return Ok(ApiResponse.Success("Organization retrieved successfully", organization));
        }

        // POST: api/MstOrganization
        [AllowAnonymous]
        [HttpPost("open/create")]
        public async Task<IActionResult> OpenCreate([FromBody] MstOrganizationCreateDto mstOrganizationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdOrganization = await _mstOrganizationService.CreateOrganizationAsync(mstOrganizationDto);
            return StatusCode(201, ApiResponse.Created("Organization created successfully", createdOrganization));
        }

        // PUT: api/MstOrganization/{id}
        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstOrganizationUpdateDto mstOrganizationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstOrganizationService.UpdateOrganizationAsync(id, mstOrganizationDto);
            return Ok(ApiResponse.Success("Organization updated successfully"));
        }

        // DELETE: api/MstOrganization/{id}
        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstOrganizationService.DeleteOrganizationAsync(id);
            return Ok(ApiResponse.Success("Organization marked as deleted successfully"));
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

            var filter = new MstOrganizationFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstOrganizationFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstOrganizationFilter();
            }

            var result = await _mstOrganizationService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Organizations filtered successfully", result));
        }
    }
}