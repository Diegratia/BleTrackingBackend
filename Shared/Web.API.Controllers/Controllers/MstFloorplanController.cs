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
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class MstFloorplanController : ControllerBase
    {
        private readonly IMstFloorplanService _mstFloorplanService;

        public MstFloorplanController(IMstFloorplanService mstFloorplanService)
        {
            _mstFloorplanService = mstFloorplanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var floorplans = await _mstFloorplanService.GetAllAsync();
            return Ok(ApiResponse.Success("Floorplans retrieved successfully", floorplans));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var floorplan = await _mstFloorplanService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Floorplan retrieved successfully", floorplan));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstFloorplanCreateDto dto)
        {
            if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                if (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0)
                {
                    errors["FloorplanImage"] = new[] { "File is empty" };
                }

                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var created = await _mstFloorplanService.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Floorplan created successfully", created));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstFloorplanUpdateDto dto)
        {
            if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                if (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0)
                {
                    errors["FloorplanImage"] = new[] { "File is empty" };
                }

                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstFloorplanService.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Floorplan updated successfully"));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstFloorplanService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Floorplan deleted successfully"));
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

            var filter = new MstFloorplanFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstFloorplanFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstFloorplanFilter();
            }

            var result = await _mstFloorplanService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floorplans filtered successfully", result));
        }

        [MinLevel(LevelPriority.SuperAdmin)]
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));
            }

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));
            }

            var floorplans = await _mstFloorplanService.ImportAsync(file);
            return Ok(ApiResponse.Success("Floorplans imported successfully", floorplans));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstFloorplanService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstFloorplan_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstFloorplanService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstFloorplan_Report.xlsx");
        }

        // OPEN

        [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> OpenGetAll()
        {
            var floorplans = await _mstFloorplanService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Floorplans retrieved successfully", floorplans));
        }

        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var floorplan = await _mstFloorplanService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Floorplan retrieved successfully", floorplan));
        }

        [AllowAnonymous]
        [HttpPost("open")]
        public async Task<IActionResult> OpenCreate([FromForm] MstFloorplanCreateDto dto)
        {
            if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                if (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0)
                {
                    errors["FloorplanImage"] = new[] { "File is empty" };
                }

                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var created = await _mstFloorplanService.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Floorplan created successfully", created));
        }

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromForm] MstFloorplanUpdateDto dto)
        {
            if (!ModelState.IsValid || (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0))
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                if (dto.FloorplanImage != null && dto.FloorplanImage.Length == 0)
                {
                    errors["FloorplanImage"] = new[] { "File is empty" };
                }

                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstFloorplanService.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Floorplan updated successfully"));
        }

        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstFloorplanService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Floorplan deleted successfully"));
        }

        [AllowAnonymous]
        [HttpPost("open/filter")]
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

            var filter = new MstFloorplanFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstFloorplanFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstFloorplanFilter();
            }

            var result = await _mstFloorplanService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floorplans filtered successfully", result));
        }
    }
}
