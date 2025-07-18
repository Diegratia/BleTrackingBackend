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
    [Authorize ("RequireAll")]
    public class TrxVisitorController : ControllerBase
    {
        private readonly ITrxVisitorService _trxVisitorService;

        public TrxVisitorController(ITrxVisitorService trxVisitorService)
        {
            _trxVisitorService = trxVisitorService;
        }

        // POST: api/TrxVisitor
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TrxVisitorCreateDto createDto)
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
                var createdTrxVisitor = await _trxVisitorService.CreateTrxVisitorAsync(createDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "TrxVisitor created successfully",
                    collection = new { data = createdTrxVisitor },
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

        // GET: api/TrxVisitor/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var trxVisitor = await _trxVisitorService.GetTrxVisitorByIdAsync(id);
                if (trxVisitor == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "TrxVisitor blacklist area not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = trxVisitor },
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

        // GET: api/TrxVisitor
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var visitors = await _trxVisitorService.GetAllTrxVisitorsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor retrieved successfully",
                    collection = new { data = visitors },
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
        public async Task<IActionResult> Update(Guid id, [FromForm] TrxVisitorUpdateDto trxVisitorDto)
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
                await _trxVisitorService.UpdateTrxVisitorAsync(id, trxVisitorDto);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor updated successfully",
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
                await _trxVisitorService.DeleteTrxVisitorAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitor deleted successfully",
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
                var result = await _trxVisitorService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "TrxVisitors filtered successfully",
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