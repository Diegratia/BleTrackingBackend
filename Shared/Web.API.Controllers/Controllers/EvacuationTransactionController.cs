using System;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [Route("api/evacuation-transaction")]
    [ApiController]
    [MinLevel(LevelPriority.Primary)]
    public class EvacuationTransactionController : ControllerBase
    {
        private readonly IEvacuationTransactionService _service;

        public EvacuationTransactionController(IEvacuationTransactionService service)
        {
            _service = service;
        }

        // GET: api/EvacuationTransaction
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Evacuation Transactions retrieved successfully", transactions));
        }

        // GET: api/EvacuationTransaction/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var transaction = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Evacuation Transaction retrieved successfully", transaction));
        }

        // PUT: api/EvacuationTransaction/{id}/confirm
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(Guid id, [FromBody] EvacuationTransactionConfirmDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }
            await _service.ConfirmAsync(id, dto);
            return StatusCode(204, ApiResponse.NoContent("Evacuation Transaction confirmed successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var filter = new EvacuationTransactionFilter();
            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<EvacuationTransactionFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new EvacuationTransactionFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Evacuation Transaction filtered successfully", result));
        }
    }
}
