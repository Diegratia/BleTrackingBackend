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
    public class MstMemberController : ControllerBase
    {
        private readonly IMstMemberService _mstMemberService;

        public MstMemberController(IMstMemberService mstMemberService)
        {
            _mstMemberService = mstMemberService;
        }

        // GET: api/MstMember
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var members = await _mstMemberService.GetAllMembersAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Members retrieved successfully",
                    collection = new { data = members },
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

        // GET: api/MstMember/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var member = await _mstMemberService.GetMemberByIdAsync(id);
                if (member == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Member not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Member retrieved successfully",
                    collection = new { data = member },
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

        // POST: api/MstMember
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] MstMemberCreateDto mstMemberDto)
        {
            if (!ModelState.IsValid || (mstMemberDto.FaceImage != null && mstMemberDto.FaceImage.Length == 0))
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
                var createdMember = await _mstMemberService.CreateMemberAsync(mstMemberDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Member created successfully",
                    collection = new { data = createdMember },
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

        // PUT: api/MstMember/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] MstMemberUpdateDto mstMemberDto)
        {
            if (!ModelState.IsValid || (mstMemberDto.FaceImage != null && mstMemberDto.FaceImage.Length == 0))
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
                await _mstMemberService.UpdateMemberAsync(id, mstMemberDto);
                return Ok(new
                {
                    success = true,
                    msg = "Member updated successfully",
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

        // DELETE: api/MstMember/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstMemberService.DeleteMemberAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Member marked as deleted successfully",
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