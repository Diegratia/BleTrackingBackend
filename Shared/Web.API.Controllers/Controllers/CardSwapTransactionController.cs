using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    [MinLevel(LevelPriority.Primary)]
    [Route("api/card-swap-transaction")]
    [ApiController]
    public class CardSwapTransactionController : ControllerBase
    {
        private readonly ICardSwapTransactionService _service;

        public CardSwapTransactionController(ICardSwapTransactionService service)
        {
            _service = service;
        }

        // GET: api/card-swap-transaction
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(ApiResponse.Success("Card swap transactions retrieved successfully", data));
        }

        // GET: api/card-swap-transaction/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _service.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Card swap transaction retrieved successfully", data));
        }

        // POST: api/card-swap-transaction
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CardSwapTransactionCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var result = await _service.CreateAsync(dto);
            return StatusCode(201, ApiResponse.Created("Card swap transaction created successfully", result));
        }

        // POST: api/card-swap-transaction/filter
        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new CardSwapTransactionFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<CardSwapTransactionFilter>(
                    request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new CardSwapTransactionFilter();
            }

            var result = await _service.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Data retrieved", result));
        }

        // Business Logic Endpoints

        // POST: api/card-swap-transaction/forward-swap (Enter Area)
        [HttpPost("forward-swap")]
        public async Task<IActionResult> PerformForwardSwap([FromBody] ForwardSwapRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var result = await _service.PerformForwardSwapAsync(request);
            return StatusCode(201, ApiResponse.Created("Forward swap performed successfully", result));
        }

        // POST: api/card-swap-transaction/reverse-swap (Exit Area - LIFO)
        [HttpPost("reverse-swap")]
        public async Task<IActionResult> PerformReverseSwap([FromBody] ReverseSwapRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return BadRequest(ApiResponse.BadRequest("Validation failed", errors));
            }

            var result = await _service.PerformReverseSwapAsync(request);
            return StatusCode(201, ApiResponse.Created("Reverse swap performed successfully", result));
        }

        // GET: api/card-swap-transaction/active-swaps/{swapChainId}
        [HttpGet("active-swaps/{swapChainId}")]
        public async Task<IActionResult> GetActiveSwaps(Guid swapChainId)
        {
            var result = await _service.GetActiveSwapsByChainAsync(swapChainId);
            return Ok(ApiResponse.Success("Active swaps retrieved successfully", result));
        }

        // GET: api/card-swap-transaction/can-reverse/{visitorId}/{swapChainId}
        [HttpGet("can-reverse/{visitorId}/{swapChainId}")]
        public async Task<IActionResult> CanReverse(Guid visitorId, Guid swapChainId)
        {
            var canReverse = await _service.CanReverseSwapAsync(visitorId, swapChainId);
            return Ok(ApiResponse.Success("Reverse availability checked", new { canReverse }));
        }

        // GET: api/card-swap-transaction/last-active/{visitorId}/{swapChainId}
        [HttpGet("last-active/{visitorId}/{swapChainId}")]
        public async Task<IActionResult> GetLastActiveSwap(Guid visitorId, Guid swapChainId)
        {
            var result = await _service.GetLastActiveSwapAsync(visitorId, swapChainId);
            if (result == null)
                return Ok(ApiResponse.Success("No active swap found", null));
            
            return Ok(ApiResponse.Success("Last active swap retrieved", result));
        }
    }
}
