using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Data.ViewModels;
using BusinessLogic.Services.Implementation;
using BusinessLogic.Services.Interface;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Repositories.Repository.RepoModel;
using Data.ViewModels.ResponseHelper;

namespace Web.API.Controllers.Controllers
{
    [Route("api/alarm-record")]
    [ApiController]
    [Authorize("RequireAllAndUserCreated")]
    public class AlarmRecordTrackingController : ControllerBase
    {
        private readonly IAlarmRecordTrackingService _service;

        public AlarmRecordTrackingController(IAlarmRecordTrackingService service)
        {
            _service = service;
        }

        [HttpPost("log")]
        public async Task<IActionResult> GetAlarmLogsAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var alarms = await _service.GetAlarmLogsAsync(request);
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
        [HttpPost("event-log")]
        public async Task<IActionResult> GetAlarmTriggerLogsAsync(AlarmAnalyticsRequestRM request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse.BadRequest("Invalid filter parameters"));
            var alarms = await _service.GetAlarmTriggerLogsAsync(request);
                return Ok(ApiResponse.Success("Alarm Events retrieved successfully", alarms));
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

        // [HttpPost("{filter}")]
        // public async Task<IActionResult> Filter([FromBody] DataTablesRequest request)
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
        //         var result = await _service.FilterAsync(request);
        //         return Ok(new
        //         {
        //             success = true,
        //             msg = "Alarm Record Tracking filtered successfully",
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

        [HttpGet("export/pdf")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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

        /******************************************************************************
        OPEN
       ********************************************************************************/
        [HttpPost("open{id}")]
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

    }
}
