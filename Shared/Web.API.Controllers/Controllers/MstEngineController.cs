using System;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.PrimaryAdmin)]
    [Route("api/[controller]")]
    [ApiController]
    public class MstEngineController : ControllerBase
    {
        private readonly IMstEngineService _mstEngineService;

        public MstEngineController(IMstEngineService mstEngineService)
        {
            _mstEngineService = mstEngineService;
        }

        /// <summary>
        /// Get all engines
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var engines = await _mstEngineService.GetAllEnginesAsync();
            return Ok(ApiResponse.Success("Engines retrieved successfully", engines));
        }

        /// <summary>
        /// Get engine by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var engine = await _mstEngineService.GetEngineByIdAsync(id);
            if (engine == null)
                return NotFound(ApiResponse.NotFound("Engine not found"));

            return Ok(ApiResponse.Success("Engine retrieved successfully", engine));
        }

        /// <summary>
        /// Create new engine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstEngineCreateDto dto)
        {
            var createdEngine = await _mstEngineService.CreateEngineAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdEngine.Id },
                ApiResponse.Created("Engine created successfully", createdEngine));
        }

        /// <summary>
        /// Update engine
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstEngineUpdateDto dto)
        {
            await _mstEngineService.UpdateEngineAsync(id, dto);
            return Ok(ApiResponse.Success("Engine updated successfully"));
        }

        /// <summary>
        /// Delete engine (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mstEngineService.DeleteEngineAsync(id);
            return Ok(ApiResponse.Success("Engine deleted successfully"));
        }

        /// <summary>
        /// Filter engines with DataTables format (legacy)
        /// </summary>
        [HttpPost("filter-datatables")]
        public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        {
            var result = await _mstEngineService.FilterAsync(request);
            return Ok(ApiResponse.Success("Engines filtered successfully", result));
        }

        /// <summary>
        /// Filter engines with new format
        /// </summary>
        [HttpPost("filter")]
        public async Task<IActionResult> FilterNew([FromBody] MstEngineFilter filter)
        {
            var (data, total, filtered) = await _mstEngineService.FilterNewAsync(filter);
            return Ok(ApiResponse.Success("Engines filtered successfully", new
            {
                data,
                total,
                filtered,
                page = filter.Page,
                pageSize = filter.PageSize
            }));
        }

        /// <summary>
        /// Export engines to PDF
        /// </summary>
        // [HttpGet("export/pdf")]
        // [AllowAnonymous]
        // public async Task<IActionResult> ExportPdf()
        // {
        //     var pdfBytes = await _mstEngineService.ExportPdfAsync();
        //     return File(pdfBytes, "application/pdf", "MstEngine_Report.pdf");
        // }

        // /// <summary>
        // /// Export engines to Excel
        // /// </summary>
        // [HttpGet("export/excel")]
        // [AllowAnonymous]
        // public async Task<IActionResult> ExportExcel()
        // {
        //     var excelBytes = await _mstEngineService.ExportExcelAsync();
        //     return File(excelBytes,
        //         "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //         "MstEngine_Report.xlsx");
        // }

        /// <summary>
        /// Stop engine via MQTT
        /// </summary>
        [HttpPost("stop/{engineId}")]
        public async Task<IActionResult> StopEngine(string engineId)
        {
            await _mstEngineService.StopEngineAsync(engineId);
            return Ok(ApiResponse.Success($"Stop command sent to engine {engineId}"));
        }

        /// <summary>
        /// Start engine via MQTT
        /// </summary>
        [HttpPost("start/{engineId}")]
        public async Task<IActionResult> StartEngine(string engineId)
        {
            await _mstEngineService.StartEngineAsync(engineId);
            return Ok(ApiResponse.Success($"Start command sent to engine {engineId}"));
        }
    }
}
