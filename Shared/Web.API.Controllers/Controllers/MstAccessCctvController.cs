using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Shared.Contracts;
using System.Text;
using System.Text.Json;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.SuperAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class MstAccessCctvController : ControllerBase
    {
        private readonly IMstAccessCctvService _mstAccessCctvService;

        public MstAccessCctvController(IMstAccessCctvService mstAccessCctvService)
        {
            _mstAccessCctvService = mstAccessCctvService;
        }

        // GET: api/MstAccessCctv
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accessCctvs = await _mstAccessCctvService.GetAllAsync();
            return Ok(ApiResponse.Success("Access CCTVs retrieved successfully", accessCctvs));
        }

        [HttpGet("unassigned")]
        public async Task<IActionResult> GetAllUnassignedAsync()
        {
            var accessCctvs = await _mstAccessCctvService.GetAllUnassignedAsync();
            return Ok(ApiResponse.Success("Unassigned Access CCTVs retrieved successfully", accessCctvs));
        }

        // GET: api/MstAccessCctv/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var accessCctv = await _mstAccessCctvService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Access CCTV retrieved successfully", accessCctv));
        }

        // POST: api/MstAccessCctv
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstAccessCctvCreateDto mstAccessCctvDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var createdAccessCctv = await _mstAccessCctvService.CreateAsync(mstAccessCctvDto);
            return StatusCode(201, ApiResponse.Created("Access CCTV created successfully", createdAccessCctv));
        }

        // PUT: api/MstAccessCctv/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstAccessCctvUpdateDto mstAccessCctvDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            await _mstAccessCctvService.UpdateAsync(id, mstAccessCctvDto);
            return Ok(ApiResponse.Success("Access CCTV updated successfully"));
        }

        // DELETE: api/MstAccessCctv/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstAccessCctvService.DeleteAsync(id);
            return Ok(ApiResponse.Success("Access CCTV deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstAccessCctvFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstAccessCctvFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new MstAccessCctvFilter();
            }

            var result = await _mstAccessCctvService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _mstAccessCctvService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "MstAccessCctv_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _mstAccessCctvService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "MstAccessCctv_Report.xlsx");
        }

        // OPEN

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var accessCctvs = await _mstAccessCctvService.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Access CCTVs retrieved successfully", accessCctvs));
        }

        [HttpPost("open/filter")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new MstAccessCctvFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<MstAccessCctvFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new MstAccessCctvFilter();
            }

            var result = await _mstAccessCctvService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }
    }
}