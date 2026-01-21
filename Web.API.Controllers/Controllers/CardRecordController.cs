using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Entities.Models;
using Repositories.Repository.RepoModel;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize("RequireSystemOrSuperAdminRole")]
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
            try
            {
                var cardRecords = await _cardRecordService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Card Records retrieved successfully",
                    collection = new { data = cardRecords },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // GET: api/MstBleReader/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var cardRecord = await _cardRecordService.GetByIdAsync(id);
                if (cardRecord == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Card Record not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = " Card Record retrieved successfully",
                    collection = new { data = cardRecord },
                    code = 200
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> Checkout(Guid id)
        {
            try
            {
                await _cardRecordService.CheckoutCard(id);
                return Ok(new
                {
                    success = true,
                    msg = "Card checked out successfully",
                    collection = new { data = (object)null },
                    code = 200
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CardRecordCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var createdCardRecord = await _cardRecordService.CreateAsync(createDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Card Record created successfully",
                    collection = new { data = createdCardRecord },
                    code = 201
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        // [HttpPost("{filter}")]
        // public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }

        //     try
        //     {
        //         var result = await _cardRecordService.FilterAsync(request);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Card Records filtered successfully",
        //             collection = result,
        //             code = 200
        //         });
        //     }
        //     catch (ArgumentException ex)
        //     {
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = ex.Message,
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var pdfBytes = await _cardRecordService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "CardRecord_Report.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate PDF: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var excelBytes = await _cardRecordService.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "CardRecord_Report.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate Excel: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
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
        public async Task<IActionResult> GetFiltered( [FromBody] DataTablesRequest request
        )
        {
            var result = await _cardRecordService.ProjectionFilterAsync(request);
            return Ok(ApiResponse.Success("Card Record filtered successfully", result));
        }

        // ============================
        // 2️⃣ Historis kartu dipakai siapa
        // ============================
        [HttpPost("history")]
        public async Task<IActionResult> GetHistory(
            [FromBody] CardRecordRequestRM request
        )
        {

            var result = await _cardRecordService.GetCardUsageHistoryAsync(request);
            return Ok(ApiResponse.Success("Card usage history retrieved", result));
        }

        [HttpPost("open/{filter}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
                return BadRequest(new
                {
                    success = false,
                    msg = "Validation failed: " + string.Join(", ", errors),
                    collection = new { data = (object)null },
                    code = 400
                });
            }

            try
            {
                var result = await _cardRecordService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Card Records filtered successfully",
                    collection = result,
                    code = 200
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Internal server error: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }
    }
}