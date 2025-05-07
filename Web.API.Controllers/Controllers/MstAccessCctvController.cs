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
    public class MstAccessCctvController : ControllerBase
    {
        private readonly IMstAccessCctvService _mstAccessCctvService;

        public MstAccessCctvController(IMstAccessCctvService mstAccessCctvService)
        {
            _mstAccessCctvService = mstAccessCctvService;
        }

        // GET: api/MstAccessCctv
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var accessCctvs = await _mstAccessCctvService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Access CCTVs retrieved successfully",
                    collection = new { data = accessCctvs },
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

        // GET: api/MstAccessCctv/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var accessCctv = await _mstAccessCctvService.GetByIdAsync(id);
                if (accessCctv == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Access CCTV not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Access CCTV retrieved successfully",
                    collection = new { data = accessCctv },
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

        // POST: api/MstAccessCctv
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstAccessCctvCreateDto mstAccessCctvDto)
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
                var createdAccessCctv = await _mstAccessCctvService.CreateAsync(mstAccessCctvDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Access CCTV created successfully",
                    collection = new { data = createdAccessCctv },
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

        // PUT: api/MstAccessCctv/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstAccessCctvUpdateDto mstAccessCctvDto)
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
                await _mstAccessCctvService.UpdateAsync(id, mstAccessCctvDto);
                return Ok(new
                {
                    success = true,
                    msg = "Access CCTV updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Access CCTV not found",
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

        // DELETE: api/MstAccessCctv/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstAccessCctvService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Access CCTV deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Access CCTV not found",
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