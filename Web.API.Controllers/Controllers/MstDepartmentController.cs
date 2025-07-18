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
    public class MstDepartmentController : ControllerBase
    {
        private readonly IMstDepartmentService _mstDepartmentService;

        public MstDepartmentController(IMstDepartmentService mstDepartmentService)
        {
            _mstDepartmentService = mstDepartmentService;
        }

        // GET: api/MstDepartment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var departments = await _mstDepartmentService.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Departments retrieved successfully",
                    collection = new { data = departments },
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

        // GET: api/MstDepartment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var department = await _mstDepartmentService.GetByIdAsync(id);
                if (department == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Department not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Department retrieved successfully",
                    collection = new { data = department },
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

        // POST: api/MstDepartment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstDepartmentCreateDto mstDepartmentDto)
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
                var createdDepartment = await _mstDepartmentService.CreateAsync(mstDepartmentDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Department created successfully",
                    collection = new { data = createdDepartment },
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

        // PUT: api/MstDepartment/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstDepartmentUpdateDto mstDepartmentDto)
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
                await _mstDepartmentService.UpdateAsync(id, mstDepartmentDto);
                return Ok(new
                {
                    success = true,
                    msg = "Department updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Department not found",
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

        // DELETE: api/MstDepartment/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstDepartmentService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Department deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Department not found",
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
                var result = await _mstDepartmentService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Departments filtered successfully",
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

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            try
            {
                var pdfBytes = await _mstDepartmentService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "MstDepartment_Report.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate PDF: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var excelBytes = await _mstDepartmentService.ExportExcelAsync();
                return File(excelBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "MstDepartment_Report.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    msg = $"Failed to generate Excel: {ex.Message}",
                    collection = new { data = (object)null },
                    code = 500
                });
            }
        }
    }
}