using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize ("RequirePrimaryAdminOrSystemRole")]
    public class VisitorBlacklistAreaController : ControllerBase
    {
        private readonly IVisitorBlacklistAreaService _visitorBlacklistAreaService;

        public VisitorBlacklistAreaController(IVisitorBlacklistAreaService visitorBlacklistAreaService)
        {
            _visitorBlacklistAreaService = visitorBlacklistAreaService;
        }

        // POST: api/VisitorBlacklistArea
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VisitorBlacklistAreaCreateDto dto)
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
                var createdBlacklistArea = await _visitorBlacklistAreaService.CreateVisitorBlacklistAreaAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Visitor blacklist area created successfully",
                    collection = new { data = createdBlacklistArea },
                    code = 201
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

        // GET: api/VisitorBlacklistArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var blacklistArea = await _visitorBlacklistAreaService.GetVisitorBlacklistAreaByIdAsync(id);
                if (blacklistArea == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Visitor blacklist area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blacklist area retrieved successfully",
                    collection = new { data = blacklistArea },
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

        // GET: api/VisitorBlacklistArea
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var blacklistAreas = await _visitorBlacklistAreaService.GetAllVisitorBlacklistAreasAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blacklist areas retrieved successfully",
                    collection = new { data = blacklistAreas },
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

        // PUT: api/VisitorBlacklistArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] VisitorBlacklistAreaUpdateDto dto)
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
                await _visitorBlacklistAreaService.UpdateVisitorBlacklistAreaAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blacklist area updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 404
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

        // DELETE: api/VisitorBlacklistArea/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _visitorBlacklistAreaService.DeleteVisitorBlacklistAreaAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor blacklist area deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    msg = ex.Message,
                    collection = new { data = (object)null },
                    code = 404
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

         [HttpPost("{filter}")]
         public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
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
                var result = await _visitorBlacklistAreaService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Visitor Blacklist Area filtered successfully",
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