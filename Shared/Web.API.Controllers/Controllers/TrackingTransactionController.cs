using System;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.Primary)]
    public class TrackingTransactionController : ControllerBase
    {
        private readonly ITrackingTransactionService _trackingTransactionService;

        public TrackingTransactionController(ITrackingTransactionService trackingTransactionService)
        {
            _trackingTransactionService = trackingTransactionService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transaction = await _trackingTransactionService.GetByIdAsync(id);
            if (transaction == null)
                return NotFound(ApiResponse.NotFound("Tracking transaction not found"));
            return Ok(ApiResponse.Success("Tracking transaction retrieved successfully", transaction));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new TrackingTransactionFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<TrackingTransactionFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new TrackingTransactionFilter();
            }

            var result = await _trackingTransactionService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Tracking transactions filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _trackingTransactionService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "TrackingTransaction_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _trackingTransactionService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "TrackingTransaction_Report.xlsx");
        }
    }
}
