using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MstIntegrationController : ControllerBase
    {
        private readonly IMstIntegrationService _mstIntegrationService;

        public MstIntegrationController(IMstIntegrationService mstIntegrationService)
        {
            _mstIntegrationService = mstIntegrationService;
        }

        // GET: api/MstIntegration
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var integrations = await _mstIntegrationService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Integrations retrieved successfully",
                    collection = new { data = integrations },
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

        // GET: api/MstIntegration/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var integration = await _mstIntegrationService.GetByIdAsync(id);
                if (integration == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Integration not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Integration retrieved successfully",
                    collection = new { data = integration },
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

        // POST: api/MstIntegration
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstIntegrationCreateDto mstIntegrationDto)
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
                var createdIntegration = await _mstIntegrationService.CreateAsync(mstIntegrationDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Integration created successfully",
                    collection = new { data = createdIntegration },
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

        // PUT: api/MstIntegration/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstIntegrationUpdateDto mstIntegrationDto)
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
                await _mstIntegrationService.UpdateAsync(id, mstIntegrationDto); // No var assignment
                return Ok(new
                {
                    success = true,
                    msg = "Integration updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Integration not found",
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

        // DELETE: api/MstIntegration/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstIntegrationService.DeleteAsync(id); // No var assignment
                return Ok(new
                {
                    success = true,
                    msg = "Integration deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Integration not found",
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
    }
}