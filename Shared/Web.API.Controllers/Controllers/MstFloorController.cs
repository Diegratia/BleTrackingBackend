using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Data.ViewModels.ResponseHelper;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using System.Text.Json;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]

    public class MstFloorController : ControllerBase
    {
        private readonly IMstFloorService _mstFloorService;

        public MstFloorController(IMstFloorService mstFloorService)
        {
            _mstFloorService = mstFloorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var floors = await _mstFloorService.GetAllAsync();
            return Ok(ApiResponse.Success("Floors retrieved successfully", floors));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var floor = await _mstFloorService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Floor retrieved successfully", floor));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstFloorCreateDto mstFloorDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdFloor = await _mstFloorService.CreateAsync(mstFloorDto);
            return StatusCode(201, ApiResponse.Created("Floor created successfully", createdFloor));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstFloorUpdateDto mstFloorDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstFloorService.UpdateAsync(id, mstFloorDto);
            return Ok(ApiResponse.Success("Floor updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstFloorService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Floor deleted successfully"));
        }

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

            var floors = await _mstFloorService.ImportAsync(file);
            return Ok(ApiResponse.Success("Floors imported successfully", floors));
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

            var filter = new MstFloorFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstFloorFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstFloorFilter();
            }

            var result = await _mstFloorService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floors filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstFloorService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstFloor_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstFloorService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstFloor_Report.xlsx");
        }

        //OPEN

         [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> OpenGetAll()
        {
            var floors = await _mstFloorService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Floors retrieved successfully", floors));
        }

        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var floor = await _mstFloorService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Floor retrieved successfully", floor));
        }

        [AllowAnonymous]
        [HttpPost("open")]
        public async Task<IActionResult> OpenCreate([FromBody] MstFloorCreateDto mstFloorDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdFloor = await _mstFloorService.CreateAsync(mstFloorDto);
            return StatusCode(201, ApiResponse.Created("Floor created successfully", createdFloor));
        }

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstFloorUpdateDto mstFloorDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstFloorService.UpdateAsync(id, mstFloorDto);
            return Ok(ApiResponse.Success("Floor updated successfully"));
        }

        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstFloorService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Floor deleted successfully"));
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

            var filter = new MstFloorFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstFloorFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstFloorFilter();
            }

            var result = await _mstFloorService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Floors filtered successfully", result));
        }

    }
}




        
        

