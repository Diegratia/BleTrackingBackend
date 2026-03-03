using System;
using System.Text.Json;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using DataView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Interface;
using BusinessLogic.Services.Extension.RootExtension;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [MinLevel(LevelPriority.SuperAdmin)]
    public class CardGroupController : ControllerBase
    {
        private readonly ICardGroupService _cardGroupService;

        public CardGroupController(ICardGroupService cardGroupService)
        {
            _cardGroupService = cardGroupService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cardGroups = await _cardGroupService.GetAllAsync();
            return Ok(ApiResponse.Success("Card Groups retrieved successfully", cardGroups));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var cardGroup = await _cardGroupService.GetByIdAsync(id);
            return Ok(ApiResponse.Success("Card Group retrieved successfully", cardGroup));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CardGroupCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            var createdCardGroup = await _cardGroupService.CreateAsync(createDto);
            return StatusCode(201, ApiResponse.Created("Card Group created successfully", createdCardGroup));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CardGroupUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value!.Errors).Select(x => x.ErrorMessage);
                return BadRequest(ApiResponse.BadRequest("Validation failed: " + string.Join(", ", errors)));
            }

            await _cardGroupService.UpdateAsync(id, dto);
            return Ok(ApiResponse.Success("Card Group updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _cardGroupService.DeleteAsync(id);
            return Ok(ApiResponse.NoContent("Card Group deleted successfully"));
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromBody] DataTablesProjectedRequest request)
        {
            var filter = new CardGroupFilter();

            if (request.Filters.ValueKind == JsonValueKind.Object)
            {
                filter = JsonSerializer.Deserialize<CardGroupFilter>(request.Filters.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new CardGroupFilter();
            }

            var result = await _cardGroupService.FilterAsync(request, filter);
            return Ok(ApiResponse.Paginated("Card Groups filtered successfully", result));
        }
    }
}
