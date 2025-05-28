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
    [Authorize]
    public class BleReaderNodeController : ControllerBase
    {
        private readonly IBleReaderNodeService _bleReaderNodeService;

        public BleReaderNodeController(IBleReaderNodeService BleReaderNodeService)
        {
            _bleReaderNodeService = BleReaderNodeService;
        }

        // GET: api/BleReaderNode
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bleReaders = await _bleReaderNodeService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "BLE Reader Nodes retrieved successfully",
                    collection = new { data = bleReaders },
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

        // GET: api/BleReaderNode/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var bleReader = await _bleReaderNodeService.GetByIdAsync(id);
                if (bleReader == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "BLE Reader Node not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "BLE Reader Node retrieved successfully",
                    collection = new { data = bleReader },
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

        // POST: api/BleReaderNode
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BleReaderNodeCreateDto BleReaderNodeDto)
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
                var createdBleReader = await _bleReaderNodeService.CreateAsync(BleReaderNodeDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "BLE Reader Node Node created successfully",
                    collection = new { data = createdBleReader },
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

        // PUT: api/BleReaderNode/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BleReaderNodeUpdateDto BleReaderNodeDto)
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
                await _bleReaderNodeService.UpdateAsync(id, BleReaderNodeDto);
                return Ok(new
                {
                    success = true,
                    msg = "BLE Reader Node Node updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "BLE Reader Node not found",
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

        // DELETE: api/BleReaderNode/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _bleReaderNodeService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "BLE Reader Node deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "BLE Reader Node not found",
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