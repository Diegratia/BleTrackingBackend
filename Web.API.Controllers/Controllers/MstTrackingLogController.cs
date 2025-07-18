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
    [Route("api/mst-tracking")]
    [ApiController]
    public class MstTrackingLogController : ControllerBase
    {
        private readonly IMstTrackingLogService _trackingService;

        public MstTrackingLogController(IMstTrackingLogService trackingService)
        {
            _trackingService = trackingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MstTrackingLogDto>>> GetMstTrackingLogs()
        {
            var logs = await _trackingService.GetMstTrackingLogsAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MstTrackingLogDto>> GetMstTrackingLog(Guid id)
        {
            var log = await _trackingService.GetMstTrackingLogByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            return Ok(log);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMstTrackingLog(Guid id)
        {
            await _trackingService.DeleteMstTrackingLogAsync(id);
            return Ok(id);
        }
    }
}