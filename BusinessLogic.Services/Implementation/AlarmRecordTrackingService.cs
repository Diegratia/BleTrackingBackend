using AutoMapper;
using BusinessLogic.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.ViewModels;
using Entities.Models;
using Repositories.Repository;

namespace BusinessLogic.Services.Implementation
{
    public class AlarmRecordTrackingService : IAlarmRecordTrackingService
    {
        private readonly AlarmRecordTrackingRepository _repository;
        private readonly IMapper _mapper;

        public AlarmRecordTrackingService(AlarmRecordTrackingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id)
        {
            var alarm = await _repository.GetByIdAsync(id);
            return alarm == null ? null : _mapper.Map<AlarmRecordTrackingDto>(alarm);
        }

        public async Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync()
        {
            var alarms = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlarmRecordTrackingDto>>(alarms);
        }

        public async Task<AlarmRecordTrackingDto> CreateAsync(AlarmRecordTrackingCreateDto createDto)
        {
            // Validasi relasi
            var visitor = await _repository.GetVisitorByIdAsync(createDto.VisitorId);
            if (visitor == null)
                throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            var reader = await _repository.GetReaderByIdAsync(createDto.ReaderId);
            if (reader == null)
                throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

            var maskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(createDto.FloorplanMaskedAreaId);
            if (maskedArea == null)
                throw new ArgumentException($"Area with ID {createDto.FloorplanMaskedAreaId} not found.");

            var app = await _repository.GetApplicationByIdAsync(createDto.ApplicationId);
            if (app == null)
                throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var alarm = _mapper.Map<AlarmRecordTracking>(createDto);

            // Set nilai default untuk properti yang tidak ada di DTO
            alarm.Id = Guid.NewGuid();
            alarm.Timestamp = DateTime.UtcNow;
            alarm.IdleTimestamp = DateTime.UtcNow;
            alarm.DoneTimestamp = DateTime.MaxValue;
            alarm.CancelTimestamp = DateTime.MaxValue;
            alarm.WaitingTimestamp = DateTime.MaxValue;
            alarm.InvestigatedTimestamp = DateTime.MaxValue;
            alarm.IdleBy = "System";
            alarm.DoneBy = "System";
            alarm.CancelBy = "System";
            alarm.WaitingBy = "System";
            alarm.InvestigatedBy = "System";
            alarm.InvestigatedDoneAt = DateTime.MaxValue;

            await _repository.AddAsync(alarm);
            return _mapper.Map<AlarmRecordTrackingDto>(alarm);
        }

        public async Task UpdateAsync(Guid id, AlarmRecordTrackingUpdateDto updateDto)
        {
            var alarm = await _repository.GetByIdAsync(id);
            if (alarm == null)
                throw new KeyNotFoundException("Alarm record not found");

            // Validasi relasi jika berubah
            if (alarm.VisitorId != updateDto.VisitorId)
            {
                var visitor = await _repository.GetVisitorByIdAsync(updateDto.VisitorId);
                if (visitor == null)
                    throw new ArgumentException($"Visitor with ID {updateDto.VisitorId} not found.");
            }

            if (alarm.ReaderId != updateDto.ReaderId)
            {
                var reader = await _repository.GetReaderByIdAsync(updateDto.ReaderId);
                if (reader == null)
                    throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");
            }

            if (alarm.FloorplanMaskedAreaId != updateDto.FloorplanMaskedAreaId)
            {
                var maskedArea = await _repository.GetFloorplanMaskedAreaByIdAsync(updateDto.FloorplanMaskedAreaId);
                if (maskedArea == null)
                    throw new ArgumentException($"Masked Area with ID {updateDto.FloorplanMaskedAreaId} not found.");
            }

            if (alarm.ApplicationId != updateDto.ApplicationId)
            {
                var app = await _repository.GetApplicationByIdAsync(updateDto.ApplicationId);
                if (app == null)
                    throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
            }

            _mapper.Map(updateDto, alarm);

            await _repository.UpdateAsync(alarm);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}