using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BusinessLogic.Services.Extension.RootExtension;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Data.ViewModels.ResponseHelper;
using Shared.Contracts;

namespace Web.API.Controllers.Controllers
{
    /// <summary>
    /// Controller for Security personnel actions on Alarm Triggers
    /// Security can: Accept dispatch, Mark as Arrived, Complete Investigation
    /// </summary>
    [MinLevel(LevelPriority.Primary)]
    [ApiController]
    [Route("api/[controller]")]
    public class AlarmTriggersSecurityController : ControllerBase
    {
        private readonly IAlarmTriggersService _service;

        public AlarmTriggersSecurityController(IAlarmTriggersService service)
        {
            _service = service;
        }

        /// <summary>
        /// Security accepts the dispatched alarm
        /// Flow: Dispatched → Accepted
        /// </summary>
        /// <param name="id">Alarm Trigger ID</param>
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            await _service.AcceptAsync(id);
            return Ok(ApiResponse.Success("Alarm accepted successfully"));
        }

        /// <summary>
        /// Security marks as arrived at location
        /// Flow: Accepted → Arrived
        /// </summary>
        /// <param name="id">Alarm Trigger ID</param>
        [HttpPut("{id}/arrived")]
        public async Task<IActionResult> Arrived(Guid id)
        {
            await _service.ArrivedAsync(id);
            return Ok(ApiResponse.Success("Arrived at location marked successfully"));
        }

        /// <summary>
        /// Security completes investigation with result
        /// Flow: Arrived → DoneInvestigated
        /// </summary>
        /// <param name="id">Alarm Trigger ID</param>
        /// <param name="dto">Contains investigation result</param>
        [HttpPut("{id}/done-investigated")]
        public async Task<IActionResult> DoneInvestigated(Guid id, [FromBody] AlarmTriggersSecurityActionDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.InvestigationResult))
            {
                return BadRequest(ApiResponse.BadRequest("Investigation result is required"));
            }

            await _service.DoneInvestigatedAsync(id, dto.InvestigationResult);
            return Ok(ApiResponse.Success("Investigation completed successfully"));
        }
    }
}
