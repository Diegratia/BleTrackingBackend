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
    [Route("api/tracking-transaction")]
    [ApiController]
    [Authorize]
    public class TrackingTransactionController : ControllerBase
    {
        private readonly ITrackingTransactionService _trackingTransactionService;

        public TrackingTransactionController(ITrackingTransactionService trackingTransactionService)
        {
            _trackingTransactionService = trackingTransactionService;
        }

        // POST: api/TrackingTransaction
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TrackingTransactionCreateDto dto)
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
                var createdTransaction = await _trackingTransactionService.CreateTrackingTransactionAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Tracking transaction created successfully",
                    collection = new { data = createdTransaction },
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

        // GET: api/TrackingTransaction/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var transaction = await _trackingTransactionService.GetTrackingTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Tracking transaction not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Tracking transaction retrieved successfully",
                    collection = new { data = transaction },
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

        // GET: api/TrackingTransaction
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var transactions = await _trackingTransactionService.GetAllTrackingTransactionsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Tracking transactions retrieved successfully",
                    collection = new { data = transactions },
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

        // PUT: api/TrackingTransaction/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TrackingTransactionUpdateDto dto)
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
                await _trackingTransactionService.UpdateTrackingTransactionAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    msg = "Tracking transaction updated successfully",
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

        // DELETE: api/TrackingTransaction/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _trackingTransactionService.DeleteTrackingTransactionAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Tracking transaction deleted successfully",
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
    }
}