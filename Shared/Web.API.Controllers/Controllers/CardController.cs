using System;
using System.Threading.Tasks;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]
    public class CardController : ControllerBase
    {
        private readonly ICardService _service;

        public CardController(ICardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cards = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Card retrieved successfully", cards));
        }

        [HttpGet("unused")]
        public async Task<IActionResult> GetAllUnUsedAsync()
        {
            var cards = await _service.GetAllUnUsedAsync();
            return Ok(ApiResponse.Success("Unused Card retrieved successfully", cards));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var card = await _service.GetByIdAsync(id);
            if (card == null)
                return NotFound(ApiResponse.NotFound("Card not found"));

            return Ok(ApiResponse.Success("Card retrieved successfully", card));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CardCreateDto cardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdCard = await _service.CreateAsync(cardDto);
            return StatusCode(201, ApiResponse.Created("Card created successfully", createdCard));
        }

        [HttpPost("v2")]
        public async Task<IActionResult> CreateWithAccess([FromBody] CardAddDto cardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdCard = await _service.CreateMinimalAsync(cardDto);
            return StatusCode(201, ApiResponse.Created("Card created successfully", createdCard));
        }
        [HttpPut("v2/{id}")]
        public async Task<IActionResult> UpdateWithAccess(Guid id, [FromBody] CardEditDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.UpdatesAsync(id, dto);
            return Ok(ApiResponse.NoContent("Assign member successfully"));
        }
        
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkAdd([FromBody] CardBulkAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            if (dto.Cards == null || !dto.Cards.Any())
                return BadRequest(ApiResponse.BadRequest("No cards provided in bulk request"));

            var result = await _service.BulkAddAsync(dto);

            return StatusCode(201, ApiResponse.Created(
                $"Bulk add completed: {result.TotalSucceeded} succeeded, {result.TotalFailed} failed",
                result));
        }

        [HttpPut("assign-member/{id}")]
        public async Task<IActionResult> AssignToMemberAsync(Guid id, [FromBody] CardAssignDto CardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.AssignToMemberAsync(id, CardDto);
            return Ok(ApiResponse.NoContent("Assign member successfully"));
        }

        [HttpPut("card-access/{id}")]
        public async Task<IActionResult> UpdateAccessAsync(Guid id, [FromBody] CardAccessEdit CardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.UpdateAccessAsync(id, CardDto);
            return Ok(ApiResponse.NoContent("Card updated successfully"));
        }

        [AllowAnonymous]
        [HttpPut("open/card-access/{cardNumber}")]
        public async Task<IActionResult> UpdateAccessByVMSAsync(string cardNumber, [FromBody] CardAccessEdit CardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.UpdateAccessByVMSAsync(cardNumber, CardDto);
            return Ok(ApiResponse.NoContent("Card updated successfully"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CardUpdateDto CardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.UpdateAsync(id, CardDto);
            return Ok(ApiResponse.NoContent("Card updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Card deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new CardFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<CardFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CardFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Success("Card filtered successfully", result));
        }

        [HttpGet("export/pdf")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportPdf()
        {
            var pdfBytes = await _service.ExportPdfAsync();
            return File(pdfBytes, "application/pdf", "Card_Report.pdf");
        }

        [HttpGet("export/excel")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportExcel()
        {
            var excelBytes = await _service.ExportExcelAsync();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Card_Report.xlsx");
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse.BadRequest("No file uploaded or file is empty"));

            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.BadRequest("Only .xlsx files are allowed"));

            var floors = await _service.ImportAsync(file);
            return Ok(ApiResponse.Success("Cards imported successfully", floors));
        }

        //OPEN ENDPOINTS

        [HttpGet("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            var cards = await _service.OpenGetAllAsync();
            return Ok(ApiResponse.Success("Card retrieved successfully", cards));
        }

        [HttpGet("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            var card = await _service.GetByIdAsync(id);
            if (card == null)
                return NotFound(ApiResponse.NotFound("Card not found"));

            return Ok(ApiResponse.Success("Card retrieved successfully", card));
        }

        [HttpPost("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenCreate([FromBody] CardCreateDto cardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdCard = await _service.CreateAsync(cardDto);
            return StatusCode(201, ApiResponse.Created("Card created successfully", createdCard));
        }

        [HttpPut("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] CardUpdateDto CardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _service.UpdateAsync(id, CardDto);
            return Ok(ApiResponse.NoContent("Card updated successfully"));
        }

        [AllowAnonymous]
        [HttpGet("open-unused")]
        public async Task<IActionResult> OpenGetAllUnUsedAsync()
        {
            var cards = await _service.OpenGetAllUnUsedAsync();
            return Ok(ApiResponse.Success("Unused Card retrieved successfully", cards));
        }

        [HttpDelete("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Card deleted successfully"));
        }

        [HttpPost("open/filter")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new CardFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<CardFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CardFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Success("Card filtered successfully", result));
        }
    }
}
