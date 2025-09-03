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

        public async Task UpdateAsync(Guid id, AlarmTriggersUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByIdAsync(id);
            if (alarmTriggers == null || alarmTriggers.IsActive == false)
                throw new KeyNotFoundException($"alarmTriggers with ID {id} not found or has been deleted.");

            // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            // alarmTriggers.UpdatedBy = username;
            // alarmTriggers.UpdatedAt = DateTime.UtcNow;
            _mapper.Map(dto, alarmTriggers);

            await _repository.UpdateAsync(alarmTriggers);
        }
        
        public async Task UpdateAlarmStatusAsync(string beaconId, AlarmTriggersUpdateDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var alarmTriggers = await _repository.GetByDmacAsync(beaconId);
            if (alarmTriggers == null)
                throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} not found.");

            foreach (var alarmTrigger in alarmTriggers)
            {
                if (alarmTrigger.IsActive == false)
                    throw new KeyNotFoundException($"alarmTriggers with beaconId {beaconId} has been deleted.");
                if (dto.ActionStatus == "investigated" || dto.ActionStatus == "postponeinvestigated" || dto.ActionStatus == "waiting")
                {
                    alarmTrigger.IsActive = true;
                }
                else
                {
                    alarmTrigger.IsActive = false;
                }
                // var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
                // alarmTrigger.UpdatedBy = username;
                // alarmTrigger.UpdatedAt = DateTime.UtcNow;
                _mapper.Map(dto, alarmTrigger);

            }
            await _repository.UpdateBatchAsync(alarmTriggers);
        }
        
            public async Task<object> FilterAsync(DataTablesRequest request)
        {
            var query = _repository.GetAllQueryable();

            var searchableColumns = new[] { "Floorplan.Name", "Beacon.Id" };
            var validSortColumns = new[] { "Floorplan.Name", "Beacon.Id", "Alarm", "Action", "IsActive" };

            var filterService = new GenericDataTableService<AlarmTriggers, AlarmTriggersDto>(
                query,
                _mapper,
                searchableColumns,
                validSortColumns);

            return await filterService.FilterAsync(request);
        }
        
    }
}