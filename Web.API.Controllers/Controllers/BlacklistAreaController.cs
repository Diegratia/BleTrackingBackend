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
    [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
    public class BlacklistAreaController : ControllerBase
    {
        private readonly IBlacklistAreaService _BlacklistAreaService;

        public BlacklistAreaController(IBlacklistAreaService BlacklistAreaService)
        {
            _BlacklistAreaService = BlacklistAreaService;
        }

        // POST: api/BlacklistArea
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlacklistAreaCreateDto dto)
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
                var createdBlacklistArea = await _BlacklistAreaService.CreateBlacklistAreaAsync(dto);
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
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCollection([FromBody] BlacklistAreaRequestDto request)
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
                var createdBlacklistArea = await _BlacklistAreaService.CreatesBlacklistAreaAsync(request);
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

        [HttpPost("batch")]
        public async Task<IActionResult> Create([FromBody] List<BlacklistAreaCreateDto> dto)
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
                var createdBlacklist = await _BlacklistAreaService.CreateBatchBlacklistAreaAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "District created successfully",
                    collection = new { data = createdBlacklist },
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

        // GET: api/BlacklistArea/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var blacklistArea = await _BlacklistAreaService.GetBlacklistAreaByIdAsync(id);
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

        // GET: api/BlacklistArea
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var blacklistAreas = await _BlacklistAreaService.GetAllBlacklistAreasAsync();
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

        // PUT: api/BlacklistArea/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BlacklistAreaUpdateDto dto)
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
                await _BlacklistAreaService.UpdateBlacklistAreaAsync(id, dto);
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

        // DELETE: api/BlacklistArea/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _BlacklistAreaService.DeleteBlacklistAreaAsync(id);
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
                var result = await _BlacklistAreaService.FilterAsync(request);
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

        // [HttpPost("{filter}-minimal")]
        // public async Task<IActionResult> MinimalFilter([FromBody] DataTablesRequest request)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage);
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = "Validation failed: " + string.Join(", ", errors),
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }

        //     try
        //     {
        //         var result = await _BlacklistAreaService.MinimalFilterAsync(request);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Visitor Blacklist Area filtered successfully",
        //             collection = result,
        //             code = 200
        //         });
        //     }
        //     catch (ArgumentException ex)
        //     {
        //         return BadRequest(new
        //         {
        //             success = false,
        //             msg = ex.Message,
        //             collection = new { data = (object)null },
        //             code = 400
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Internal server error: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        //OPEN

        [HttpGet("open")]
         [AllowAnonymous]
        public async Task<IActionResult> OpenGetAll()
        {
            try
            {
                var blacklistAreas = await _BlacklistAreaService.OpenGetAllBlacklistAreasAsync();
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

        // PUT: api/BlacklistArea/{id}
        [HttpPut("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenUpdate(Guid id, [FromBody] BlacklistAreaUpdateDto dto)
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
                await _BlacklistAreaService.UpdateBlacklistAreaAsync(id, dto);
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

        // DELETE: api/BlacklistArea/{id}
        [HttpDelete("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenDelete(Guid id)
        {
            try
            {
                await _BlacklistAreaService.DeleteBlacklistAreaAsync(id);
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

        [HttpPost("open/{filter}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenFilter([FromBody] DataTablesRequest request)
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
                var result = await _BlacklistAreaService.FilterAsync(request);
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


        [HttpPost("open")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenCreate([FromBody] BlacklistAreaCreateDto dto)
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
                var createdBlacklistArea = await _BlacklistAreaService.CreateBlacklistAreaAsync(dto);
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

        // GET: api/BlacklistArea/{id}
        [HttpGet("open/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> OpenGetById(Guid id)
        {
            try
            {
                var blacklistArea = await _BlacklistAreaService.GetBlacklistAreaByIdAsync(id);
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

    }
}