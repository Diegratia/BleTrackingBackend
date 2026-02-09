using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Data.ViewModels.ResponseHelper;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using Repositories.Repository.RepoModel;
using Shared.Contracts;
using Shared.Contracts.Read;
using DataView;

namespace BusinessLogic.Services.Implementation
{
    public class AlarmTriggersService : BaseService, IAlarmTriggersService
    {
        private readonly AlarmTriggersRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;

        public AlarmTriggersService(
            AlarmTriggersRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
        }

        public async Task<IEnumerable<AlarmTriggersRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync()
        {
            return await _repository.GetAllLookUpAsync();
        }

        public async Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync()
        {
            var alarmTriggers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlarmTriggersOpenDto>>(alarmTriggers);
        }

        public async Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto)
        {
            var username = UsernameFormToken;
            var action = dto.ActionStatus?.Trim().ToLowerInvariant();
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByIdEntityAsync(id);
            if (alarmTriggers == null)
                throw new NotFoundException($"alarmTriggers with Id {id} not found.");

            if (alarmTriggers.IsActive == false)
                throw new BusinessException("Alarm is no longer active.");

            if (action == "postponeinvestigated")
            {
                alarmTriggers.IsActive = true;
                alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
            }
            else if (action == "investigated")
            {
                if (!dto.AssignedSecurityId.HasValue)
                    throw new BusinessException("Security must be assigned when investigated.");

                if (alarmTriggers.SecurityId != null)
                    throw new BusinessException("Alarm already assigned to a security.");

                // Ownership validation for SecurityId
                var invalidSecurityIds = await _repository.CheckInvalidSecurityOwnershipAsync(dto.AssignedSecurityId.Value, AppId);
                if (invalidSecurityIds.Any())
                    throw new UnauthorizedException($"SecurityId does not belong to this Application: {string.Join(", ", invalidSecurityIds)}");

                var security = await _repository.GetSecurityByIdAsync(dto.AssignedSecurityId.Value);
                if (security == null)
                    throw new BusinessException("Assigned security not found.");

                alarmTriggers.IsActive = true;
                alarmTriggers.SecurityId = security.Id;
                alarmTriggers.InvestigatedBy = username;
                alarmTriggers.InvestigatedTimestamp = DateTime.UtcNow;
                alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
            }
            else if (action == "noaction")
            {
                alarmTriggers.IsActive = false;
                alarmTriggers.CancelBy = username;
                alarmTriggers.CancelTimestamp = DateTime.UtcNow;
                alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
            }
            else if (action == "waiting")
            {
                alarmTriggers.IsActive = false;
                alarmTriggers.WaitingBy = username;
                alarmTriggers.WaitingTimestamp = DateTime.UtcNow;
                alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
            }
            else if (action == "done")
            {
                if (string.IsNullOrWhiteSpace(dto.InvestigatedResult))
                    throw new BusinessException("Investigated result must be provided before marking alarm as done.");

                if (!string.IsNullOrWhiteSpace(alarmTriggers.InvestigatedResult))
                    throw new BusinessException("Alarm has already been completed.");

                alarmTriggers.InvestigatedResult = dto.InvestigatedResult;
                alarmTriggers.IsActive = false;
                alarmTriggers.DoneBy = username;
                alarmTriggers.DoneTimestamp = DateTime.UtcNow;
                alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
            }

            await _repository.UpdateAsync(alarmTriggers);
            await _audit.Updated("AlarmTriggers", alarmTriggers.Id, $"Alarm {alarmTriggers.BeaconId} updated with action: {action}");
        }
        
        public async Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto)
        {
            var username = UsernameFormToken;
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByDmacAsync(beaconId);
            if (!alarmTriggers.Any())
                throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} not found.");

            foreach (var alarmTrigger in alarmTriggers)
            {
                if (alarmTrigger.IsActive == false)
                    throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} has been deleted.");

                if (dto.ActionStatus == "postponeinvestigated")
                {
                    alarmTrigger.IsActive = true;
                    alarmTrigger.ActionUpdatedAt = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "investigated")
                {
                    alarmTrigger.IsActive = true;
                    alarmTrigger.InvestigatedBy = username;
                    alarmTrigger.InvestigatedTimestamp = DateTime.UtcNow;
                    alarmTrigger.ActionUpdatedAt = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "noaction")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.CancelBy = username;
                    alarmTrigger.CancelTimestamp = DateTime.UtcNow;
                    alarmTrigger.ActionUpdatedAt = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "waiting")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.WaitingBy = username;
                    alarmTrigger.WaitingTimestamp = DateTime.UtcNow;
                    alarmTrigger.ActionUpdatedAt = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "done")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.DoneBy = username;
                    alarmTrigger.DoneTimestamp = DateTime.UtcNow;
                    alarmTrigger.ActionUpdatedAt = DateTime.UtcNow;
                }
            }

            await _repository.UpdateBatchAsync(alarmTriggers);
            await _audit.Updated("AlarmTriggers", Guid.Empty, $"Alarm batch for beacon {beaconId} updated with action: {dto.ActionStatus}");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request)
        {
            var filter = new AlarmTriggersFilter
            {
                Search = request.SearchValue,
                Page = request.Start / request.Length + 1,
                PageSize = request.Length,
                SortColumn = request.SortColumn,
                SortDir = request.SortDir ?? "desc"
            };

            var result = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = result.Total,
                recordsFiltered = result.Filtered,
                data = result.Data
            };
        }

        public async Task AcknowledgeAsync(Guid id, string username)
        {
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.AcknowledgedAt.HasValue)
                throw new BusinessException("Alarm already acknowledged");

            alarm.AcknowledgedAt = DateTime.UtcNow;
            alarm.AcknowledgedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
            await _audit.Updated("AlarmTriggers", id, $"Alarm acknowledged by {username}");
        }

        public async Task DispatchedAsync(Guid id, string username)
        {
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.AcknowledgedAt == null)
                throw new BusinessException("Cannot mark dispatched: alarm not acknowledged yet");

            if (alarm.DispatchedAt.HasValue)
                throw new BusinessException("Alarm already marked as dispatched");

            alarm.DispatchedAt = DateTime.UtcNow;
            alarm.DispatchedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
            await _audit.Updated("AlarmTriggers", id, $"Security {username} dispatched to location");
        }

        public async Task ArrivedAsync(Guid id, string username)
        {
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.DispatchedAt == null)
                throw new BusinessException("Cannot mark arrived: security not dispatched yet");

            if (alarm.ArrivedAt.HasValue)
                throw new BusinessException("Alarm already marked as arrived");

            alarm.ArrivedAt = DateTime.UtcNow;
            alarm.ArrivedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
            await _audit.Updated("AlarmTriggers", id, $"Security {username} arrived at location");
        }

        public async Task<object> GetIncidentTimelineAsync(Guid alarmTriggerId)
        {
            var alarmTrigger = await _repository.GetIncidentTimelineAsync(alarmTriggerId);

            if (alarmTrigger == null)
                throw new NotFoundException($"Alarm trigger with ID {alarmTriggerId} not found");

            // Build incident info
            var incidentInfo = new IncidentInfoDto
            {
                AlarmTriggerId = alarmTrigger.Id,
                TriggerTime = alarmTrigger.TriggerTime,
                AlarmColor = alarmTrigger.AlarmColor,
                AlarmStatus = alarmTrigger.Alarm?.ToString() ?? "Unknown",
                ActionStatus = alarmTrigger.Action?.ToString() ?? "Unknown",
                IsActive = alarmTrigger.IsActive,
                IsInRestrictedArea = alarmTrigger.IsInRestrictedArea,
                Location = new IncidentLocationDto
                {
                    FloorplanId = alarmTrigger.FloorplanId,
                    FloorplanName = alarmTrigger.Floorplan?.Name,
                    FloorplanMaskedAreaId = null,
                    AreaName = null,
                    Position = new IncidentPositionDto
                    {
                        X = alarmTrigger.PosX,
                        Y = alarmTrigger.PosY,
                        BeaconId = alarmTrigger.BeaconId
                    }
                },
                Person = new IncidentPersonDto
                {
                    Type = alarmTrigger.VisitorId.HasValue ? "Visitor" :
                           alarmTrigger.MemberId.HasValue ? "Member" : "Unknown",
                    Id = alarmTrigger.VisitorId ?? alarmTrigger.MemberId,
                    Name = alarmTrigger.Visitor?.Name ?? alarmTrigger.Member?.Name,
                    IdentityId = alarmTrigger.Visitor?.IdentityId ?? alarmTrigger.Member?.IdentityId,
                    CardNumber = alarmTrigger.Visitor?.CardNumber ?? alarmTrigger.Member?.CardNumber
                },
                Security = new IncidentSecurityDto
                {
                    Id = alarmTrigger.SecurityId,
                    Name = alarmTrigger.Security?.Name,
                    Email = alarmTrigger.Security?.Email
                }
            };

            // Build timeline events
            var timeline = BuildTimelineEvents(alarmTrigger);

            // Calculate durations
            var duration = CalculateDurations(timeline);

            // Build investigation info
            var investigation = new IncidentInvestigationDto
            {
                Result = alarmTrigger.InvestigatedResult,
                InvestigatedBy = alarmTrigger.InvestigatedBy,
                InvestigatedById = alarmTrigger.SecurityId,
                InvestigatedAt = alarmTrigger.InvestigatedTimestamp,
                DoneAt = alarmTrigger.DoneTimestamp,
                WasInvestigated = alarmTrigger.InvestigatedTimestamp.HasValue
            };

            var response = new IncidentTimelineResponseDto
            {
                IncidentInfo = incidentInfo,
                Timeline = timeline,
                Duration = duration,
                Investigation = investigation
            };

            return ApiResponse.Success("Incident timeline retrieved successfully", response);
        }

        private List<IncidentTimelineEventDto> BuildTimelineEvents(AlarmTriggers alarm)
        {
            var timeline = new List<IncidentTimelineEventDto>();

            // Stage 1: Triggered
            timeline.Add(new IncidentTimelineEventDto
            {
                Stage = "triggered",
                Timestamp = alarm.TriggerTime ?? DateTime.UtcNow,
                Actor = null,
                ActorId = null,
                DurationInSeconds = 0,
                DurationFormatted = "0 seconds",
                Description = $"Alarm triggered by beacon {alarm.BeaconId ?? "Unknown"}",
            });

            // Calculate previous timestamp for duration
            DateTime? previousTimestamp = alarm.TriggerTime;

            // Stage 2: Acknowledged (if exists)
            if (alarm.AcknowledgedAt.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.AcknowledgedAt.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "acknowledged",
                    Timestamp = alarm.AcknowledgedAt.Value,
                    Actor = alarm.AcknowledgedBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Alarm acknowledged by {alarm.AcknowledgedBy ?? "Security"}",
                });

                previousTimestamp = alarm.AcknowledgedAt.Value;
            }

            // Stage 3: Dispatched (if exists)
            if (alarm.DispatchedAt.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.DispatchedAt.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "dispatched",
                    Timestamp = alarm.DispatchedAt.Value,
                    Actor = alarm.DispatchedBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"{alarm.DispatchedBy ?? "Security"} dispatched to location",
                });

                previousTimestamp = alarm.DispatchedAt.Value;
            }

            // Stage 4: Arrived (if exists)
            if (alarm.ArrivedAt.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.ArrivedAt.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "arrived",
                    Timestamp = alarm.ArrivedAt.Value,
                    Actor = alarm.ArrivedBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"{alarm.ArrivedBy ?? "Security"} arrived at location",
                });

                previousTimestamp = alarm.ArrivedAt.Value;
            }

            // Stage 5: Waiting (if exists)
            if (alarm.WaitingTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.WaitingTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "waiting",
                    Timestamp = alarm.WaitingTimestamp.Value,
                    Actor = alarm.WaitingBy,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Put in waiting queue by {alarm.WaitingBy ?? "System"}",
                });

                previousTimestamp = alarm.WaitingTimestamp.Value;
            }

            // Stage 4: Investigated (if exists)
            if (alarm.InvestigatedTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.InvestigatedTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "investigating",
                    Timestamp = alarm.InvestigatedTimestamp.Value,
                    Actor = alarm.InvestigatedBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Investigation started by {alarm.InvestigatedBy ?? "Security"}",
                });

                previousTimestamp = alarm.InvestigatedTimestamp.Value;
            }

            // Stage 5: Done/Resolved (if exists)
            if (alarm.DoneTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.DoneTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "done",
                    Timestamp = alarm.DoneTimestamp.Value,
                    Actor = alarm.DoneBy,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Marked as done by {alarm.DoneBy ?? "System"}",
                });

                previousTimestamp = alarm.DoneTimestamp.Value;
            }

            // Stage 6: Cancelled (if exists)
            if (alarm.CancelTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.CancelTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "cancelled",
                    Timestamp = alarm.CancelTimestamp.Value,
                    Actor = alarm.CancelBy,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Cancelled by {alarm.CancelBy ?? "System"}",
                });
            }

            // Stage 7: Idle (if exists)
            if (alarm.IdleTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.IdleTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "idle",
                    Timestamp = alarm.IdleTimestamp.Value,
                    Actor = alarm.IdleBy,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Marked as idle by {alarm.IdleBy ?? "System"}",
                });
            }

            return timeline.OrderBy(t => t.Timestamp).ToList();
        }

        private IncidentDurationDto CalculateDurations(List<IncidentTimelineEventDto> timeline)
        {
            if (!timeline.Any())
            {
                return new IncidentDurationDto();
            }

            var firstEvent = timeline.OrderBy(t => t.Timestamp).First();
            var lastEvent = timeline.OrderByDescending(t => t.Timestamp).First();

            var totalSeconds = (lastEvent.Timestamp - firstEvent.Timestamp).TotalSeconds;

            // Response time: Trigger to first action (notified, waiting, or investigated)
            var firstActionEvent = timeline.Skip(1).FirstOrDefault();
            var responseSeconds = firstActionEvent != null
                ? (firstActionEvent.Timestamp - firstEvent.Timestamp).TotalSeconds
                : (double?)null;

            // Resolution time: From first action to last event
            var resolutionSeconds = firstActionEvent != null && timeline.Count > 2
                ? (lastEvent.Timestamp - firstActionEvent.Timestamp).TotalSeconds
                : (double?)null;

            return new IncidentDurationDto
            {
                TotalSeconds = totalSeconds,
                TotalFormatted = FormatDuration(totalSeconds),
                ResponseTimeSeconds = responseSeconds,
                ResponseTimeFormatted = responseSeconds.HasValue ? FormatDuration(responseSeconds.Value) : null,
                ResolutionTimeSeconds = resolutionSeconds,
                ResolutionTimeFormatted = resolutionSeconds.HasValue ? FormatDuration(resolutionSeconds.Value) : null
            };
        }

        private string FormatDuration(double seconds)
        {
            if (seconds < 60)
            {
                return $"{(int)seconds} seconds";
            }
            else if (seconds < 3600)
            {
                var minutes = (int)(seconds / 60);
                var remainingSeconds = (int)(seconds % 60);
                return remainingSeconds > 0
                    ? $"{minutes} minutes {remainingSeconds} seconds"
                    : $"{minutes} minutes";
            }
            else
            {
                var hours = (int)(seconds / 3600);
                var minutes = (int)((seconds % 3600) / 60);
                return minutes > 0
                    ? $"{hours} hours {minutes} minutes"
                    : $"{hours} hours";
            }
        }

    }
}