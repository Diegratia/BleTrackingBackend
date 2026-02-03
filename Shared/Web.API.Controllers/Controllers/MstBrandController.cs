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
    public class MstBrandController : ControllerBase
    {
        private readonly IMstBrandService _mstBrandService;

        public MstBrandController(IMstBrandService mstBrandService)
        {
            _mstBrandService = mstBrandService;
        }

        // GET: api/MstBrand
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _mstBrandService.GetAllAsync();
            return Ok(ApiResponse.Success("Brands retrieved successfully", brands));
        }

        // GET: api/MstBrand/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var brand = await _mstBrandService.GetByIdAsync(id);
            if (brand == null) return NotFound(ApiResponse.NotFound("Brand not found"));
            return Ok(ApiResponse.Success("Brand retrieved successfully", brand));
        }

        // POST: api/MstBrand
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstBrandCreateDto mstBrandDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdBrand = await _mstBrandService.CreateAsync(mstBrandDto);
            return StatusCode(201, ApiResponse.Created("Brand created successfully", createdBrand));
        }

        // PUT: api/MstBrand/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstBrandUpdateDto mstBrandDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstBrandService.UpdateAsync(id, mstBrandDto);
            return Ok(ApiResponse.Success("Brand updated successfully"));
        }

        // DELETE: api/MstBrand/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstBrandService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Brand marked as deleted successfully"));
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

            var filter = new MstBrandFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstBrandFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstBrandFilter();
            }

            var result = await _mstBrandService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Brands filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstBrandService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstBrand_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstBrandService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstBrand_Report.xlsx");
        }

        // OPEN APIs

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var brands = await _mstBrandService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Brands retrieved successfully", brands));
        }

        [AllowAnonymous]
        [HttpGet("open/{id}")]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var brand = await _mstBrandService.GetByIdAsync(id);
            if (brand == null) return NotFound(ApiResponse.NotFound("Brand not found"));
            return Ok(ApiResponse.Success("Brand retrieved successfully", brand));
        }

        [AllowAnonymous]
        [HttpPost("open/create")]
        public async Task<IActionResult> OpenCreate([FromBody] MstBrandCreateDto mstBrandDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdBrand = await _mstBrandService.CreateAsync(mstBrandDto);
            return StatusCode(201, ApiResponse.Created("Brand created successfully", createdBrand));
        }

        [AllowAnonymous]
        [HttpPut("open/{id}")]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] MstBrandUpdateDto mstBrandDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstBrandService.UpdateAsync(id, mstBrandDto);
            return Ok(ApiResponse.Success("Brand updated successfully"));
        }

        [AllowAnonymous]
        [HttpDelete("open/{id}")]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _mstBrandService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Brand marked as deleted successfully"));
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

            var filter = new MstBrandFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstBrandFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new MstBrandFilter();
            }

            var result = await _mstBrandService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Brands filtered successfully", result));
        }
    }
}
