using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;
using AutoMapper;

namespace Web.API.Controllers.Controllers
{
    [Route("")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public UserGroupController(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var groups = await _context.UserGroups.ToListAsync();
                var groupDtos = _mapper.Map<List<UserGroupDto>>(groups);
                return Ok(new
                {
                    success = true,
                    msg = "User groups retrieved successfully",
                    collection = new { data = groupDtos },
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var group = await _context.UserGroups.FindAsync(id);
                if (group == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "User group not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                var groupDto = _mapper.Map<UserGroupDto>(group);
                return Ok(new
                {
                    success = true,
                    msg = "User group retrieved successfully",
                    collection = new { data = groupDto },
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