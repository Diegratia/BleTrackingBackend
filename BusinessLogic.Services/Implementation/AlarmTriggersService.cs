using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using Repositories.Repository;

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
        
    }
}