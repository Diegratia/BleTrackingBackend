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
    [Route("api/record-tracking")]
    [ApiController]
    public class RecordTrackingLogController : ControllerBase
    {
        private readonly IRecordTrackingLogService _trackingService;

        public RecordTrackingLogController(IRecordTrackingLogService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecordTrackingLogDto>>> GetRecordTrackingLogs()
        {
            var records = await _trackingService.GetRecordTrackingLogsAsync();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecordTrackingLogDto>> GetRecordTrackingLog(Guid id)
        {
            var record = await _trackingService.GetRecordTrackingLogByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }
            return Ok(record);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecordTrackingLog(Guid id)
        {
            await _trackingService.DeleteRecordTrackingLogAsync(id);
            return Ok(id);
        }
    }
}