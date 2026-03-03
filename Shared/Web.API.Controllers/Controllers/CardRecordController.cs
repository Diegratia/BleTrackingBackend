using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.PrimaryAdmin)]
    public class CardRecordController : ControllerBase
    {
        private readonly ICardRecordService _cardRecordService;

        public CardRecordController(ICardRecordService cardRecordService)
        {
            _cardRecordService = cardRecordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cardRecords = await _cardRecordService.GetAllAsync();
            return Ok(ApiResponse.Success("Card Records retrieved successfully", cardRecords));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cardRecord = await _cardRecordService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Card Record retrieved successfully", cardRecord));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CardRecordCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdCardRecord = await _cardRecordService.CreateAsync(createDto);
            return StatusCode(201, ApiResponse.Created("Card Record created successfully", createdCardRecord));
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> Checkout(Guid id)
        {
            await _cardRecordService.CheckoutCard(id);
            return Ok(ApiResponse.NoContent("Card checked out successfully"));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _cardRecordService.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "CardRecord_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _cardRecordService.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "CardRecord_Report.xlsx");
        }

        // ============================
        // 1️⃣ Berapa kali kartu dipakai
        // ============================
        [HttpGet("usage")]
        public async Task<IActionResult> GetUsage()
        {
            var result = await _cardRecordService.GetCardUsageSummaryAsync();
            return Ok(ApiResponse.Success("Card usage summary retrieved", result));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new Shared.Contracts.CardRecordFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<Shared.Contracts.CardRecordFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new Shared.Contracts.CardRecordFilter();
            }

            var result = await _cardRecordService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Card Record filtered successfully", result));
        }

        // ============================
        // 2️⃣ Historis kartu dipakai siapa
        // ============================
        [HttpPost("history")]
        public async Task<IActionResult> GetHistory([FromBody] CardRecordRequestRM request)
        {
            var result = await _cardRecordService.GetCardUsageHistoryAsync(request);
            return Ok(ApiResponse.Success("Card usage history retrieved", result));
        }
    }
}
