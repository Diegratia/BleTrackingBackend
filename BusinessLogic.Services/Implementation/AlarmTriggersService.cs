using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;
using System.ComponentModel.DataAnnotations;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Implementation
{
    public class AlarmTriggersService : IAlarmTriggersService
    {

        private readonly AlarmTriggersRepository _repository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AlarmTriggersService(AlarmTriggersRepository repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<AlarmTriggersDto>> GetAllAsync()
        {
            var alarmTriggers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlarmTriggersDto>>(alarmTriggers);
        }
        public async Task<IEnumerable<AlarmTriggersLookUp>> GetAllLookUpAsync()
        {
            var alarmTriggers = await _repository.GetAllLookUpAsync();
            return _mapper.Map<IEnumerable<AlarmTriggersLookUp>>(alarmTriggers);
        }
        public async Task<IEnumerable<AlarmTriggersOpenDto>> OpenGetAllAsync()
        {
            var alarmTriggers = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlarmTriggersOpenDto>>(alarmTriggers);
        }

        public async Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByIdAsync(id);
            if (alarmTriggers == null)
                    throw new KeyNotFoundException($"alarmTriggers with beaconId {alarmTriggers} not found.");

                            if (alarmTriggers.IsActive == false)
                                throw new KeyNotFoundException($"alarmTriggers with beaconId {alarmTriggers} has been deleted.");
                            if (dto.ActionStatus == "postponeinvestigated")
                            {
                                alarmTriggers.IsActive = true;
                            }
                            else if (dto.ActionStatus == "investigated")
                            {
                                alarmTriggers.IsActive = true;
                                alarmTriggers.InvestigatedBy = username;
                                alarmTriggers.InvestigatedTimestamp = DateTime.UtcNow;
                            }
                            else if (dto.ActionStatus == "noaction")
                            {
                                alarmTriggers.IsActive = false;
                                alarmTriggers.CancelBy = username;
                                alarmTriggers.CancelTimestamp = DateTime.UtcNow;
                            }
                            else if (dto.ActionStatus == "waiting")
                            {
                                alarmTriggers.IsActive = false;
                                alarmTriggers.WaitingBy = username;
                                alarmTriggers.WaitingTimestamp = DateTime.UtcNow;
                            }
                            else if (dto.ActionStatus == "done")
                            {
                                alarmTriggers.IsActive = false;
                                alarmTriggers.DoneBy = username;
                                alarmTriggers.DoneTimestamp = DateTime.UtcNow;
                            }
                    
                            _mapper.Map(dto, alarmTriggers);
            await _repository.UpdateAsync(alarmTriggers);
        }
        
        public async Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto)
        {
            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByDmacAsync(beaconId);
            if (alarmTriggers == null)
                throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} not found.");

            foreach (var alarmTrigger in alarmTriggers)
            {
                if (alarmTrigger.IsActive == false)
                    throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} has been deleted.");
                if (dto.ActionStatus == "postponeinvestigated")
                {
                    alarmTrigger.IsActive = true;
                }
                else if (dto.ActionStatus == "investigated")
                {
                    alarmTrigger.IsActive = true;
                    alarmTrigger.InvestigatedBy = username;
                    alarmTrigger.InvestigatedTimestamp = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "noaction")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.CancelBy = username;
                    alarmTrigger.CancelTimestamp = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "waiting")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.WaitingBy = username;
                    alarmTrigger.WaitingTimestamp = DateTime.UtcNow;
                }
                else if (dto.ActionStatus == "done")
                {
                    alarmTrigger.IsActive = false;
                    alarmTrigger.DoneBy = username;
                    alarmTrigger.DoneTimestamp = DateTime.UtcNow;
                }
         
                _mapper.Map(dto, alarmTrigger);

            }
            await _repository.UpdateBatchAsync(alarmTriggers);
        }
        
            public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Floorplan.Name", "BeaconId", "Visitor.Name", "Member.Name" };
            var validSortColumns = new[] { "TriggerTime","IdleTimestamp","Floorplan.Name", "Beacon.Id", "Alarm", "Action", "IsActive" };

            var filterService = new GenericDataTableService<AlarmTriggers, AlarmTriggersDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}