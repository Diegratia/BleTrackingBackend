using Microsoft.AspNetCore.Mvc;
using Quartz;
using System.Threading.Tasks;

namespace BusinessLogic.Services.JobsScheduler
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsSchedulerController : ControllerBase
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public JobsSchedulerController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        [HttpPost("trigger-create-table")]
        public async Task<IActionResult> TriggerCreateTable()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(new JobKey("CreateDailyTrackingTable", "Tracking"));
            return Ok($"CreateDailyTrackingTableJob triggered at {DateTime.UtcNow:UTC}");
        }
    }
}