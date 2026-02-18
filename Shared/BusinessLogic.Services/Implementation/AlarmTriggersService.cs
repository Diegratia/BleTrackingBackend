using AutoMapper;
using BusinessLogic.Services.Extension.RootExtension;
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
using System.Text.Json;

namespace BusinessLogic.Services.Implementation
{
    public class AlarmTriggersService : BaseService, IAlarmTriggersService
    {
        private readonly AlarmTriggersRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditEmitter _audit;
        private readonly IUserService _userService;

        public AlarmTriggersService(
            AlarmTriggersRepository repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IAuditEmitter audit,
            IUserService userService) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _audit = audit;
            _userService = userService;
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
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasAlarmActionPermission())
                throw new UnauthorizedException("User does not have alarm action permission");

            var username = UsernameFormToken;
            var action = dto.ActionStatus?.Trim().ToLowerInvariant();
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByIdEntityAsync(id);
            if (alarmTriggers == null)
                throw new NotFoundException($"alarmTriggers with Id {id} not found.");

            if (alarmTriggers.IsActive == false && action != "done" && action != "resolve")
                throw new BusinessException("Alarm is no longer active.");

            switch (action)
            {
                // New workflow actions
                case "ack":
                case "acknowledge":
                    if (alarmTriggers.Action != Shared.Contracts.ActionStatus.Idle)
                        throw new BusinessException($"Cannot acknowledge: alarm is not in Idle status. Current: {alarmTriggers.Action}");
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.Acknowledged;
                    alarmTriggers.AcknowledgedAt = DateTime.UtcNow;
                    alarmTriggers.AcknowledgedBy = username;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                case "dispatch":
                    if (alarmTriggers.Action != Shared.Contracts.ActionStatus.Acknowledged)
                        throw new BusinessException($"Cannot dispatch: alarm must be acknowledged first. Current: {alarmTriggers.Action}");
                    if (!dto.AssignedSecurityId.HasValue)
                        throw new BusinessException("SecurityId is required for dispatch");
                    var invalidSecurityIds = await _repository.CheckInvalidSecurityOwnershipAsync(dto.AssignedSecurityId.Value, AppId);
                    if (invalidSecurityIds.Any())
                        throw new UnauthorizedException($"SecurityId does not belong to this Application");
                    var security = await _repository.GetSecurityByIdAsync(dto.AssignedSecurityId.Value);
                    if (security == null)
                        throw new BusinessException("Assigned security not found");
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.Dispatched;
                    alarmTriggers.SecurityId = security.Id;
                    alarmTriggers.DispatchedAt = DateTime.UtcNow;
                    alarmTriggers.DispatchedBy = username;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                case "waiting":
                    if (alarmTriggers.Action != Shared.Contracts.ActionStatus.Acknowledged)
                        throw new BusinessException($"Cannot put in waiting: alarm must be acknowledged first. Current: {alarmTriggers.Action}");
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.Waiting;
                    alarmTriggers.IsActive = false;
                    alarmTriggers.WaitingBy = username;
                    alarmTriggers.WaitingTimestamp = DateTime.UtcNow;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                case "done":
                case "resolve":
                    if (alarmTriggers.Action != Shared.Contracts.ActionStatus.DoneInvestigated)
                        throw new BusinessException($"Cannot resolve: alarm must have investigation completed first. Current: {alarmTriggers.Action}");
                    if (string.IsNullOrWhiteSpace(alarmTriggers.InvestigatedResult))
                        throw new BusinessException("Cannot resolve: no investigation result found");
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.Done;
                    alarmTriggers.IsActive = false;
                    alarmTriggers.DoneBy = username;
                    alarmTriggers.DoneTimestamp = DateTime.UtcNow;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                // Legacy actions
                case "postponeinvestigated":
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.PostponeInvestigated;
                    alarmTriggers.IsActive = true;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                case "noaction":
                    alarmTriggers.Action = Shared.Contracts.ActionStatus.NoAction;
                    alarmTriggers.IsActive = false;
                    alarmTriggers.CancelBy = username;
                    alarmTriggers.CancelTimestamp = DateTime.UtcNow;
                    alarmTriggers.ActionUpdatedAt = DateTime.UtcNow;
                    break;

                default:
                    throw new BusinessException($"Invalid action: {action}. Valid actions: ack, dispatch, waiting, done, postponeinvestigated, noaction");
            }

            await _repository.UpdateAsync(alarmTriggers);
            _audit.Updated("AlarmTriggers", alarmTriggers.Id, $"Alarm {alarmTriggers.BeaconId} updated with action: {action}");
        }

        public async Task<object> FilterAsync(DataTablesProjectedRequest request, AlarmTriggersFilter filter)
        {
            filter.Page = (request.Start / request.Length) + 1;
            filter.PageSize = request.Length;
            filter.SortColumn = request.SortColumn ?? "TriggerTime";
            filter.SortDir = request.SortDir ?? "desc";
            filter.Search = request.SearchValue;

            if (request.DateFilters != null)
            {
                if (request.DateFilters.TryGetValue("TriggerTime", out var dateFilter))
                {
                    filter.TriggerTimeFrom = dateFilter.DateFrom;
                    filter.TriggerTimeTo = dateFilter.DateTo;
                }
                if (request.DateFilters.TryGetValue("ActionUpdatedAt", out var actionDateFilter))
                {
                    filter.ActionUpdatedAtFrom = actionDateFilter.DateFrom;
                    filter.ActionUpdatedAtTo = actionDateFilter.DateTo;
                }
            }

            var result = await _repository.FilterAsync(filter);

            return new
            {
                draw = request.Draw,
                recordsTotal = result.Total,
                recordsFiltered = result.Filtered,
                data = result.Data
            };
        }

        // =====================================================
        // NEW WORKFLOW METHODS
        // =====================================================

        public async Task AcknowledgeAsync(Guid id)
        {
            // Cek permission menggunakan extension method
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasAlarmActionPermission())
                throw new UnauthorizedException("Anda tidak memiliki akses alarm action");

            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Idle)
                throw new BusinessException($"Cannot acknowledge: alarm is not in Idle status. Current: {alarm.Action}");

            alarm.Action = Shared.Contracts.ActionStatus.Acknowledged;
            alarm.AcknowledgedAt = DateTime.UtcNow;
            alarm.AcknowledgedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Alarm acknowledged by {username}");
        }

        /// <summary>
        /// Operator dispatches alarm to specific security
        /// Flow: Acknowledged → Dispatched
        /// </summary>
        public async Task DispatchAsync(Guid id, Guid assignedSecurityId)
        {
            // Cek permission menggunakan extension method
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasAlarmActionPermission())
                throw new UnauthorizedException("Anda tidak memiliki akses alarm action");

            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Acknowledged)
                throw new BusinessException($"Cannot dispatch: alarm must be acknowledged first. Current: {alarm.Action}");

            var invalidSecurityIds = await _repository.CheckInvalidSecurityOwnershipAsync(assignedSecurityId, AppId);
            if (invalidSecurityIds.Any())
                throw new UnauthorizedException($"SecurityId does not belong to this Application: {string.Join(", ", invalidSecurityIds)}");

            var security = await _repository.GetSecurityByIdAsync(assignedSecurityId);
            if (security == null)
                throw new BusinessException("Assigned security not found");

            alarm.Action = Shared.Contracts.ActionStatus.Dispatched;
            alarm.SecurityId = security.Id;
            alarm.DispatchedAt = DateTime.UtcNow;
            alarm.DispatchedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Alarm dispatched to {security.Name} by {username}");
        }

        /// <summary>
        /// Operator puts alarm in waiting queue (no security available)
        /// Flow: Acknowledged → Waiting
        /// </summary>
        public async Task WaitingAsync(Guid id)
        {
            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Acknowledged)
                throw new BusinessException($"Cannot put in waiting: alarm must be acknowledged first. Current: {alarm.Action}");

            alarm.Action = Shared.Contracts.ActionStatus.Waiting;
            alarm.IsActive = false;
            alarm.WaitingBy = username;
            alarm.WaitingTimestamp = DateTime.UtcNow;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Alarm put in waiting queue by {username}");
        }

        /// <summary>
        /// Security accepts the dispatch
        /// Flow: Dispatched → Accepted
        /// </summary>
        public async Task AcceptAsync(Guid id)
        {
            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Dispatched)
                throw new BusinessException($"Cannot accept: alarm must be dispatched first. Current: {alarm.Action}");

            if (alarm.SecurityId == null)
                throw new BusinessException("Cannot accept: alarm has no assigned security");

            var currentSecurityId = GetCurrentSecurityUserId();
            if (currentSecurityId == null || currentSecurityId != alarm.SecurityId.Value)
                throw new UnauthorizedException("Cannot accept: you are not the assigned security for this alarm");

            alarm.Action = Shared.Contracts.ActionStatus.Accepted;
            alarm.AcceptedAt = DateTime.UtcNow;
            alarm.AcceptedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Alarm accepted by security {username}");
        }

        /// <summary>
        /// Security arrives at location
        /// Flow: Accepted → Arrived
        /// </summary>
        public async Task ArrivedAsync(Guid id)
        {
            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Accepted)
                throw new BusinessException($"Cannot mark arrived: alarm must be accepted first. Current: {alarm.Action}");

            var currentSecurityId = GetCurrentSecurityUserId();
            if (currentSecurityId == null || currentSecurityId != alarm.SecurityId)
                throw new UnauthorizedException("Cannot mark arrived: you are not the assigned security for this alarm");

            alarm.Action = Shared.Contracts.ActionStatus.Arrived;
            alarm.ArrivedAt = DateTime.UtcNow;
            alarm.ArrivedBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Security {username} arrived at location");
        }

        /// <summary>
        /// Security completes investigation with result
        /// Flow: Arrived → DoneInvestigated
        /// </summary>
        public async Task DoneInvestigatedAsync(Guid id, string result)
        {
            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.Arrived)
                throw new BusinessException($"Cannot complete investigation: alarm must be arrived first. Current: {alarm.Action}");

            if (string.IsNullOrWhiteSpace(result))
                throw new BusinessException("Investigation result is required");

            var currentSecurityId = GetCurrentSecurityUserId();
            if (currentSecurityId == null || currentSecurityId != alarm.SecurityId)
                throw new UnauthorizedException("Cannot complete investigation: you are not the assigned security for this alarm");

            alarm.Action = Shared.Contracts.ActionStatus.DoneInvestigated;
            alarm.InvestigatedResult = result;
            alarm.InvestigatedDoneAt = DateTime.UtcNow;
            alarm.InvestigatedDoneBy = username;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Investigation completed by {username}. Result: {result}");
        }

        /// <summary>
        /// Operator marks alarm as resolved
        /// Flow: DoneInvestigated → Done (final state)
        /// </summary>
        public async Task ResolveAsync(Guid id)
        {
            var currentUser = await _userService.GetFromTokenAsync();
            if (currentUser == null)
                throw new UnauthorizedException("User not found");

            if (!currentUser.HasAlarmActionPermission())
                throw new UnauthorizedException("User does not have alarm action permission");

            var username = UsernameFormToken;
            var alarm = await _repository.GetByIdEntityAsync(id);
            if (alarm == null)
                throw new NotFoundException($"Alarm with ID {id} not found");

            if (alarm.Action != Shared.Contracts.ActionStatus.DoneInvestigated)
                throw new BusinessException($"Cannot resolve: alarm must have investigation completed first. Current: {alarm.Action}");

            if (string.IsNullOrWhiteSpace(alarm.InvestigatedResult))
                throw new BusinessException("Cannot resolve: no investigation result found");

            alarm.Action = Shared.Contracts.ActionStatus.Done;
            alarm.IsActive = false;
            alarm.DoneBy = username;
            alarm.DoneTimestamp = DateTime.UtcNow;
            alarm.ActionUpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(alarm);
             _audit.Updated("AlarmTriggers", id, $"Alarm resolved by {username}");
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        private Guid? GetCurrentSecurityUserId()
        {
            // Try to get SecurityId from JWT token claims
            var securityIdClaim = Http.HttpContext?.User?.FindFirst("SecurityId")?.Value;
            if (Guid.TryParse(securityIdClaim, out var securityId))
                return securityId;

            var username = UsernameFormToken;

            return null;
        }

        // =====================================================
        // INCIDENT TIMELINE
        // =====================================================

        public async Task<object> GetIncidentTimelineAsync(Guid alarmTriggerId)
        {
            var alarmTrigger = await _repository.GetIncidentTimelineAsync(alarmTriggerId);

            if (alarmTrigger == null)
                throw new NotFoundException($"Alarm trigger with ID {alarmTriggerId} not found");

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

            var timeline = BuildTimelineEvents(alarmTrigger);

            var duration = CalculateDurations(timeline);

            var investigation = new IncidentInvestigationDto
            {
                Result = alarmTrigger.InvestigatedResult,
                DispatchedPerson = alarmTrigger.InvestigatedDoneBy ?? alarmTrigger.AcceptedBy,
                DispatchedPersonId = alarmTrigger.SecurityId,
                InvestigatedAt = alarmTrigger.InvestigatedDoneAt ?? alarmTrigger.ArrivedAt,
                DoneAt = alarmTrigger.DoneTimestamp,
                WasInvestigated = alarmTrigger.InvestigatedDoneAt.HasValue || alarmTrigger.ArrivedAt.HasValue
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

            DateTime? previousTimestamp = alarm.TriggerTime;

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
                    ActorId = UserIdFromToken,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Alarm acknowledged by {alarm.AcknowledgedBy ?? "Operator"}",
                });

                previousTimestamp = alarm.AcknowledgedAt.Value;
            }

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
                    Description = $"Dispatched to {alarm.AcceptedBy ?? "Security"} by {alarm.DispatchedBy ?? "Operator"}",
                });

                previousTimestamp = alarm.DispatchedAt.Value;
            }

            // Stage 4: Waiting (if exists)
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

            // Stage 5: Accepted (if exists)
            if (alarm.AcceptedAt.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.AcceptedAt.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "accepted",
                    Timestamp = alarm.AcceptedAt.Value,
                    Actor = alarm.AcceptedBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Dispatch accepted by {alarm.AcceptedBy ?? "Security"}",
                });

                previousTimestamp = alarm.AcceptedAt.Value;
            }

            // Stage 6: Arrived (if exists)
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

            // Stage 7: DoneInvestigated (if exists)
            if (alarm.InvestigatedDoneAt.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.InvestigatedDoneAt.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "done_investigated",
                    Timestamp = alarm.InvestigatedDoneAt.Value,
                    Actor = alarm.InvestigatedDoneBy,
                    ActorId = alarm.SecurityId,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Investigation completed by {alarm.InvestigatedDoneBy ?? "Security"}",
                });

                previousTimestamp = alarm.InvestigatedDoneAt.Value;
            }

            // Stage 8: Done/Resolved (if exists)
            if (alarm.DoneTimestamp.HasValue)
            {
                double? duration = null;
                if (previousTimestamp.HasValue)
                {
                    duration = (alarm.DoneTimestamp.Value - previousTimestamp.Value).TotalSeconds;
                }

                timeline.Add(new IncidentTimelineEventDto
                {
                    Stage = "resolved",
                    Timestamp = alarm.DoneTimestamp.Value,
                    Actor = alarm.DoneBy,
                    DurationInSeconds = duration,
                    DurationFormatted = duration.HasValue ? FormatDuration(duration.Value) : null,
                    Description = $"Marked as resolved by {alarm.DoneBy ?? "System"}",
                });

                previousTimestamp = alarm.DoneTimestamp.Value;
            }

            // Stage 9: Cancelled (if exists)
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

            // Response time: Trigger to first action (acknowledged, waiting, or dispatched)
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
