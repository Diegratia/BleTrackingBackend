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
    public class MstBleReaderController : ControllerBase
    {
        private readonly IMstBleReaderService _service;

        public MstBleReaderController(IMstBleReaderService mstBleReaderService)
        {
            _service = mstBleReaderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bleReaders = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("BLE Readers retrieved successfully", bleReaders));
        }

        [HttpGet("unassigned")]
        public async Task<IActionResult> GetAllUnassignedAsync()
        {
            var bleReaders = await _service.GetAllUnassignedAsync();
            return Ok(ApiResponse.Success("BLE Readers retrieved successfully", bleReaders));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var bleReader = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("BLE Reader retrieved successfully", bleReader));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstBleReaderCreateDto mstBleReaderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdBleReader = await _service.CreateAsync(mstBleReaderDto);
            return StatusCode(201, ApiResponse.Created("BLE Reader created successfully", createdBleReader));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstBleReaderUpdateDto mstBleReaderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, mstBleReaderDto);
            return Ok(ApiResponse.Success("BLE Reader updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Success("BLE Reader deleted successfully"));
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

            var filter = new MstBleReaderFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstBleReaderFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new MstBleReaderFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Ble Readers filtered successfully", result));
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var bleReaders = await _service.ImportAsync(file);
            return Ok(ApiResponse.Success("Ble readers imported successfully", bleReaders));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _service.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstBleReader_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _service.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstBleReader_Report.xlsx");
        }

        // OPEN

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var bleReaders = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("BLE Readers retrieved successfully", bleReaders));
        }

        [HttpGet("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var bleReader = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("BLE Reader retrieved successfully", bleReader));
        }

        [HttpPost("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenCreate([FromBody] MstBleReaderCreateDto mstBleReaderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdBleReader = await _service.CreateAsync(mstBleReaderDto);
            return StatusCode(201, ApiResponse.Created("BLE Reader created successfully", createdBleReader));
        }

        [HttpPut("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstBleReaderUpdateDto mstBleReaderDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _service.UpdateAsync(id, mstBleReaderDto);
            return Ok(ApiResponse.Success("BLE Reader updated successfully"));
        }

        [HttpDelete("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Success("BLE Reader deleted successfully"));
        }

        [HttpPost("open/filter")]
        [AllowAnonymous]
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

            var filter = new MstBleReaderFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstBleReaderFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new MstBleReaderFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Ble Readers filtered successfully", result));
        }
    }
}
