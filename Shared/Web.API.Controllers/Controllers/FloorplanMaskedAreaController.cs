using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class FloorplanMaskedAreaController : ControllerBase
    {
        private readonly IFloorplanMaskedAreaService _service;

        public FloorplanMaskedAreaController(IFloorplanMaskedAreaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var areas = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Areas retrieved successfully", areas));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var area = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Area retrieved successfully", area));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] FloorplanMaskedAreaCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdArea = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Area created successfully", createdArea));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] FloorplanMaskedAreaUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Area updated successfully"));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(ApiResponse.Success("Area deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new FloorplanMaskedAreaFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<FloorplanMaskedAreaFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new FloorplanMaskedAreaFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floorplan masked areas filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _service.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "FloorplanMaskedArea_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _service.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "FloorplanMaskedArea_Report.xlsx");
        }

        // OPEN

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var areas = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Areas retrieved successfully", areas));
        }

        [HttpGet("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var area = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Area retrieved successfully", area));
        }

        [AllowAnonymous]
        [HttpPost("open")]
        public async Task<IActionResult> OpenCreate([FromBody] FloorplanMaskedAreaCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdArea = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Area created successfully", createdArea));
        }

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] FloorplanMaskedAreaUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Area updated successfully"));
        }

        [AllowAnonymous]
        [HttpPost("open/filter")]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new FloorplanMaskedAreaFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<FloorplanMaskedAreaFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new FloorplanMaskedAreaFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floorplan masked areas filtered successfully", result));
        }
    }
}
