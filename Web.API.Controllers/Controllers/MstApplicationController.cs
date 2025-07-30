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
    [Authorize ("RequireSystemOrSuperAdminRole")]
    [Route("api/[controller]")]
    [ApiController]
    public class MstApplicationController : ControllerBase
    {
        private readonly IMstApplicationService _mstApplicationService;

        public MstApplicationController(IMstApplicationService mstApplicationService)
        {
            _mstApplicationService = mstApplicationService;
        }

        // GET: api/MstApplication
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var applications = await _mstApplicationService.GetAllApplicationsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Applications retrieved successfully",
                    collection = new { data = applications },
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

        // GET: api/MstApplication/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var application = await _mstApplicationService.GetApplicationByIdAsync(id);
                if (application == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Application not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "Application retrieved successfully",
                    collection = new { data = application },
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

        // POST: api/MstApplication
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MstApplicationCreateDto mstApplicationDto)
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
                var createdApplication = await _mstApplicationService.CreateApplicationAsync(mstApplicationDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Application created successfully",
                    collection = new { data = createdApplication },
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

        // PUT: api/MstApplication/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MstApplicationUpdateDto mstApplicationDto)
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
                await _mstApplicationService.UpdateApplicationAsync(id, mstApplicationDto);
                return Ok(new
                {
                    success = true,
                    msg = "Application updated successfully",
                    collection = new { data = (object)null }, // No data returned for PUT
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Application not found",
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

        // DELETE: api/MstApplication/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _mstApplicationService.DeleteApplicationAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "Application marked as deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Application not found",
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
                var result = await _mstApplicationService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Applications filtered successfully",
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
                var pdfBytes = await _mstApplicationService.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "MstApplication_Report.pdf");
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
                var excelBytes = await _mstApplicationService.ExportExcelAsync();
                return File(excelBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    "MstApplication_Report.xlsx");
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