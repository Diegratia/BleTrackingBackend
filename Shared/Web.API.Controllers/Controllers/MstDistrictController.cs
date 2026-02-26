using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension; // For MinLevelAttribute
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper; // For ApiResponse
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts; // For LevelPriority

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]
    public class MstDistrictController : ControllerBase
    {
        private readonly IMstDistrictService _mstDistrictService;

        public MstDistrictController(IMstDistrictService mstDistrictService)
        {
            _mstDistrictService = mstDistrictService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var floors = await _mstDistrictService.ImportAsync(file);
            return Ok(ApiResponse.Success("Districts imported successfully", floors));
        }

        // GET: api/MstDistrict
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var districts = await _mstDistrictService.GetAllAsync();
            return Ok(ApiResponse.Success("Districts retrieved successfully", districts));
        }

        // GET: api/MstDistrict/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var district = await _mstDistrictService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("District retrieved successfully", district));
        }

        // POST: api/MstDistrict
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstDistrictCreateDto mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdDistrict = await _mstDistrictService.CreateAsync(mstDistrictDto);
            return StatusCode(201, ApiResponse.Created("District created successfully", createdDistrict));
        }

        [HttpPost("batch")]
        public async Task<IActionResult> Create([FromBody] List<MstDistrictCreateDto> mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdDistrict = await _mstDistrictService.CreateBatchAsync(mstDistrictDto);
            return StatusCode(201, ApiResponse.Created("District created successfully", createdDistrict));
        }

        // PUT: api/MstDistrict/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstDistrictUpdateDto mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstDistrictService.UpdateAsync(id, mstDistrictDto);
            return Ok(ApiResponse.Success("District updated successfully"));
        }

        // DELETE: api/MstDistrict/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstDistrictService.DeleteAsync(id);
            return Ok(ApiResponse.Success("District deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            // Note: Validation model state for DataTablesRequest usually manual or handled by binder
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                   kvp => kvp.Key,
                   kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
               );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new MstDistrictFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstDistrictFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstDistrictFilter();
            }

            var result = await _mstDistrictService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Districts filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstDistrictService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstDistrict_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstDistrictService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstDistrict_Report.xlsx");
        }

        //OPEN APIs (Assuming they need to stay Open/AllowAnonymous)

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var districts = await _mstDistrictService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Districts retrieved successfully", districts));
        }

        // GET: api/MstDistrict/{id}
        [HttpGet("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var district = await _mstDistrictService.GetByIdAsync(id);
            // Note: Service might generally not throw for GetById nullable rets, so manual check ok or move check to service
            if (district == null) return NotFound(ApiResponse.NotFound("District not found"));
            return Ok(ApiResponse.Success("District retrieved successfully", district));
        }

        // POST: api/MstDistrict
        [HttpPost("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenCreate([FromBody] MstDistrictCreateDto mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var createdDistrict = await _mstDistrictService.CreateAsync(mstDistrictDto);
            return StatusCode(201, ApiResponse.Created("District created successfully", createdDistrict));
        }

        [HttpPost("open/batch")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenCreate([FromBody] List<MstDistrictCreateDto> mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            var createdDistrict = await _mstDistrictService.CreateBatchAsync(mstDistrictDto);
            return StatusCode(201, ApiResponse.Created("District created successfully", createdDistrict));
        }

        // PUT: api/MstDistrict/{id}
        [AllowAnonymous] // Assuming open needs to be open
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstDistrictUpdateDto mstDistrictDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstDistrictService.UpdateAsync(id, mstDistrictDto);
            return Ok(ApiResponse.Success("District updated successfully"));
        }

        // DELETE: api/MstDistrict/{id}
        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstDistrictService.DeleteAsync(id);
            return Ok(ApiResponse.Success("District deleted successfully"));
        }

        [AllowAnonymous]
        [HttpPost("open/filter")] // Note: original code had this route. Keep as is? Usually filter is POST.
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

            var filter = new MstDistrictFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstDistrictFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstDistrictFilter();
            }

            var result = await _mstDistrictService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Districts filtered successfully", result));
        }
    }
}
