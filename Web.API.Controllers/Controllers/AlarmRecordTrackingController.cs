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
    [Authorize ("RequireSystemOrSuperAdminRole")]
    public class AlarmRecordTrackingController : ControllerBase
    {
        private readonly IAlarmRecordTrackingService _service;

        public AlarmRecordTrackingController(IAlarmRecordTrackingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var alarms = await _service.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    msg = "Alarm retrieved successfully",
                    collection = new { data = alarms },
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
                var alarm = await _service.GetByIdAsync(id);
                if (alarm == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        msg = "Alarm not found",
                        collection = new { data = (object)null },
                        code = 404
                    });
                }
                return Ok(new
                {
                    success = true,
                    msg = " Alarm retrieved successfully",
                    collection = new { data = alarm },
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AlarmRecordTrackingCreateDto createDto)
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
                var alarm = await _service.CreateAsync(createDto);
                return StatusCode(201, new
                {
                    success = true,
                    msg = "Alarm created successfully",
                    collection = new { data = alarm },
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AlarmRecordTrackingUpdateDto updateDto)
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
                await _service.UpdateAsync(id, updateDto);
                return Ok(new
                {
                    success = true,
                    msg = "Alarm updated successfully",
                    collection = new { data = (object)null },
                    code = 204
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    success = false,
                    msg = "Alarm not found",
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

        // [HttpDelete("{id}")]
        // public async Task<IActionResult> Delete(Guid id)
        // {
        //     try
        //     {
        //         await _service.DeleteAsync(id);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Alarm deleted successfully",
        //             collection = new { data = (object)null },
        //             code = 204
        //         });
        //     }
        //     catch (KeyNotFoundException)
        //     {
        //         return NotFound(new
        //         {
        //             success = false,
        //             msg = "Alarm not found",
        //             collection = new { data = (object)null },
        //             code = 404
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
                var result = await _service.FilterAsync(request);
                return Ok(new
                {
                    success = true,
                    msg = "Alarm Record Tracking filtered successfully",
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
                var pdfBytes = await _service.ExportPdfAsync();
                return File(pdfBytes, "application/pdf", "AlarmRecordTracking_Report.pdf");
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
                var excelBytes = await _service.ExportExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "AlarmRecordTracking_Report.xlsx");
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
