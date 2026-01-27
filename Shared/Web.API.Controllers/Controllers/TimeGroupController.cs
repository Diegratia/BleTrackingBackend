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
    [Authorize("RequirePrimaryAdminOrSystemOrSuperAdminRole")]
    [Route("api/[controller]")]
    [ApiController]
    public class TimeGroupController : ControllerBase
    {
        private readonly ITimeGroupService _timeGroupService;

        public TimeGroupController(ITimeGroupService timeGroupService)
        {
            _timeGroupService = timeGroupService;
        }

        // GET: api/MstTimeGroup
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var timeGroup = await _timeGroupService.GetAllsAsync();
                return Ok(new
                {
                    success = true,
                    msg = "TimeGroup retrieved successfully",
                    collection = new { data = timeGroup },
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

        // GET: api/MstBrand/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var timeGroup = await _timeGroupService.GetByIdAsync(id);
                if (timeGroup == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "TimeGroup not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = "TimeGroup retrieved successfully",
                    collection = new { data = timeGroup },
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

        // POST: api/MstBrand
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TimeGroupCreateDto dto)
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
                var createdBrand = await _timeGroupService.CreateAsync(dto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "TimeGroup created successfully",
                    collection = new { data = createdBrand },
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

        // PUT: api/MstBrand/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TimeGroupUpdateDto dto)
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
                await _timeGroupService.UpdateAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    msg = "TimeGroup updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "TimeGroup not found",
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

        // DELETE: api/MstBrand/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _timeGroupService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    msg = "TimeGroup deleted successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "TimeGroup not found",
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
                var result = await _timeGroupService.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Brands filtered successfully",
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

        // [HttpGet("export/pdf")]
        // [AllowAnonymous]
        // public async Task<IActionResult> ExportPdf()
        // {
        //     try
        //     {
        //         var pdfBytes = await _timeGroupService.ExportPdfAsync();
        //         return File(pdfBytes, "application/pdf", "MstBrand_Report.pdf");
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Failed to generate PDF: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        // [HttpGet("export/excel")]
        // [AllowAnonymous]
        // public async Task<IActionResult> ExportExcel()
        // {
        //     try
        //     {
        //         var excelBytes = await _timeGroupService.ExportExcelAsync();
        //         return File(excelBytes,
        //             "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //             "MstBrand_Report.xlsx");
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new
        //         {
        //             success = false,
        //             msg = $"Failed to generate Excel: {ex.Message}",
        //             collection = new { data = (object)null },
        //             code = 500
        //         });
        //     }
        // }

        //OPEN

        // [HttpGet("open")]
        // [AllowAnonymous]
        // public async Task<IActionResult> OpenGetAll()
        // {
        //     try
        //     {
        //         var brands = await _timeGroupService.OpenGetAllAsync();
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Brands retrieved successfully",
        //             collection = new { data = brands },
        //             code = 200
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

    }
}
