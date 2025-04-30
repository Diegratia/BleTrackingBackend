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
        private readonly BleTrackingDbContext _context;
        private readonly IMapper _mapper;

        public AlarmRecordTrackingService(BleTrackingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AlarmRecordTrackingDto> GetByIdAsync(Guid id)
        {
            var alarm = await _context.AlarmRecordTrackings
                .Include(a => a.Application)
                .Include(a => a.Visitor)
                .Include(a => a.Reader)
                .Include(a => a.FloorplanMaskedArea)
                .FirstOrDefaultAsync(a => a.Id == id);
            return alarm == null ? null : _mapper.Map<AlarmRecordTrackingDto>(alarm);
        }

        public async Task<IEnumerable<AlarmRecordTrackingDto>> GetAllAsync()
        {
            var alarms = await _context.AlarmRecordTrackings
                .Include(a => a.Application)
                .Include(a => a.Visitor)
                .Include(a => a.Reader)
                .Include(a => a.FloorplanMaskedArea)
                .ToListAsync();
            return _mapper.Map<IEnumerable<AlarmRecordTrackingDto>>(alarms);
        }

        public async Task<AlarmRecordTrackingDto> CreateAsync(AlarmRecordTrackingCreateDto createDto)
        {
            // Validasi relasi
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == createDto.VisitorId);
            if (visitor == null) throw new ArgumentException($"Visitor with ID {createDto.VisitorId} not found.");

            var reader = await _context.MstBleReaders.FirstOrDefaultAsync(r => r.Id == createDto.ReaderId);
            if (reader == null) throw new ArgumentException($"Reader with ID {createDto.ReaderId} not found.");

            var maskedArea = await _context.FloorplanMaskedAreas.FirstOrDefaultAsync(a => a.Id == createDto.FloorplanMaskedAreaId);
            if (maskedArea == null) throw new ArgumentException($"Area with ID {createDto.FloorplanMaskedAreaId} not found.");

            var app = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == createDto.ApplicationId);
            if (app == null) throw new ArgumentException($"Application with ID {createDto.ApplicationId} not found.");

            var alarm = _mapper.Map<AlarmRecordTracking>(createDto);

         // Set nilai default untuk properti yang tidak ada di DTO
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

            _context.AlarmRecordTrackings.Add(alarm);
            await _context.SaveChangesAsync();

            var savedAlarm = await _context.AlarmRecordTrackings
                .Include(a => a.Application)
                .Include(a => a.Visitor)
                .Include(a => a.Reader)
                .Include(a => a.FloorplanMaskedArea)
                .FirstOrDefaultAsync(a => a.Id == alarm.Id);
            return _mapper.Map<AlarmRecordTrackingDto>(savedAlarm);
        }

        public async Task UpdateAsync(Guid id, AlarmRecordTrackingUpdateDto updateDto)
        {
            var alarm = await _context.AlarmRecordTrackings.FindAsync(id);
            if (alarm == null) throw new KeyNotFoundException("Alarm record not found");

            // Validasi relasi jika berubah
            if (alarm.VisitorId != updateDto.VisitorId)
            {
                var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.Id == updateDto.VisitorId);
                if (visitor == null) throw new ArgumentException($"Visitor with ID {updateDto.VisitorId} not found.");
            }

            if (alarm.ReaderId != updateDto.ReaderId)
            {
                var reader = await _context.MstBleReaders.FirstOrDefaultAsync(r => r.Id == updateDto.ReaderId);
                if (reader == null) throw new ArgumentException($"Reader with ID {updateDto.ReaderId} not found.");
            }

            if (alarm.FloorplanMaskedAreaId != updateDto.FloorplanMaskedAreaId)
            {
                var maskedArea = await _context.FloorplanMaskedAreas.FirstOrDefaultAsync(a => a.Id == updateDto.FloorplanMaskedAreaId);
                if (maskedArea == null) throw new ArgumentException($"Masked Area with ID {updateDto.FloorplanMaskedAreaId} not found.");
            }

            if (alarm.ApplicationId != updateDto.ApplicationId)
            {
                var app = await _context.MstApplications.FirstOrDefaultAsync(a => a.Id == updateDto.ApplicationId);
                if (app == null) throw new ArgumentException($"Application with ID {updateDto.ApplicationId} not found.");
            }

            _mapper.Map(updateDto, alarm);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var alarm = await _context.AlarmRecordTrackings.FindAsync(id);
            if (alarm == null) throw new KeyNotFoundException("Alarm record not found");

            _context.AlarmRecordTrackings.Remove(alarm);
            await _context.SaveChangesAsync();
        }
    }
}