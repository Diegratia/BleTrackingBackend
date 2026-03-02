// File: BusinessLogic/Services/Implementation/Analytics/TrackingReportPresetService.cs
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DataView;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingReportPresetService : BaseService, ITrackingReportPresetService
    {
        private readonly TrackingReportPresetRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingReportPresetService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TrackingReportPresetService(
            IHttpContextAccessor httpContextAccessor,
            TrackingReportPresetRepository repository,
            IMapper mapper,
            ILogger<TrackingReportPresetService> logger) : base(httpContextAccessor)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // 1. APPLY PRESET - Get analytics request from preset
        public async Task<TrackingAnalyticsFilter> ApplyPresetAsync(Guid presetId)
        {
            var preset = await _repository.GetByIdAsync(presetId);
            if (preset == null)
                throw new NotFoundException($"Preset with ID {presetId} not found");

            return new TrackingAnalyticsFilter
            {
                TimeRange = preset.TimeRange,
                From = preset.IsCustomRange ? preset.CustomFromDate : null,
                To = preset.IsCustomRange ? preset.CustomToDate : null,
                BuildingId = preset.BuildingId,
                FloorplanId = preset.FloorplanId,
                FloorId = preset.FloorId,
                AreaId = preset.AreaId,
                VisitorId = preset.VisitorId,
                MemberId = preset.MemberId,
                ReportTitle = preset.Name,
                // Timezone = preset.Timezone
            };
        }

        // 2. GET ALL PRESETS
        public async Task<List<PresetDto>> GetAllAsync()
        {
            var presets = await _repository.GetAllAsync();
            return _mapper.Map<List<PresetDto>>(presets);
        }

        // 3. GET BY ID
        public async Task<PresetDto> GetByIdAsync(Guid id)
        {
            var preset = await _repository.GetByIdAsync(id);
            if (preset == null)
                throw new NotFoundException($"Preset with ID {id} not found");
            return _mapper.Map<PresetDto>(preset);
        }

        // 4. SAVE PRESET
        public async Task<PresetDto> SavePresetAsync(CreateCustomPresetRequest request)
        {
            // VALIDASI: Custom preset harus punya tanggal
            if (request.TimeRange == "custom" && (!request.FromDate.HasValue || !request.ToDate.HasValue))
            {
                throw new ArgumentException("Custom preset requires both FromDate and ToDate");
            }

            // VALIDASI: FromDate harus <= ToDate
            if (request.TimeRange == "custom" && request.FromDate > request.ToDate)
            {
                throw new ArgumentException("FromDate cannot be after ToDate");
            }

            var username = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            var preset = new TrackingReportPreset
            {
                Name = request.Name,
                TimeRange = request.TimeRange,
                BuildingId = request.BuildingId,
                FloorplanId = request.FloorplanId,
                FloorId = request.FloorId,
                AreaId = request.AreaId,
                VisitorId = request.VisitorId,
                MemberId = request.MemberId,
                CustomFromDate = request.TimeRange == "custom" ? request.FromDate : null,
                CustomToDate = request.TimeRange == "custom" ? request.ToDate : null,
                CreatedBy = username,
                UpdatedBy = username,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(preset);
            return _mapper.Map<PresetDto>(created);
        }

        // 5. UPDATE PRESET
        public async Task<PresetDto> UpdateAsync(Guid id, UpdatePresetRequest request)
        {
            var username = UsernameFormToken;

            var preset = await _repository.GetByIdAsync(id);
            if (preset == null)
                throw new NotFoundException($"Preset with ID {id} not found");

            // Update fields if provided (gunakan pattern yang lebih clean)
            if (!string.IsNullOrEmpty(request.Name))
                preset.Name = request.Name;

            if (!string.IsNullOrEmpty(request.TimeRange))
            {
                preset.TimeRange = request.TimeRange;

                // Jika ganti ke custom, set dates
                if (request.TimeRange == "custom")
                {
                    preset.CustomFromDate = request.FromDate;
                    preset.CustomToDate = request.ToDate;
                }
                else // Jika ganti ke daily/weekly/monthly, clear custom dates
                {
                    preset.CustomFromDate = null;
                    preset.CustomToDate = null;
                }
            }
            else if (preset.TimeRange == "custom")
            {
                // Tetap custom, update dates jika diberikan
                if (request.FromDate.HasValue)
                    preset.CustomFromDate = request.FromDate;

                if (request.ToDate.HasValue)
                    preset.CustomToDate = request.ToDate;
            }

            // Update filter fields (gunakan null-coalescing assignment)
            preset.BuildingId = request.BuildingId ?? preset.BuildingId;
            preset.FloorplanId = request.FloorplanId ?? preset.FloorplanId;
            preset.FloorId = request.FloorId ?? preset.FloorId;
            preset.AreaId = request.AreaId ?? preset.AreaId;
            preset.VisitorId = request.VisitorId ?? preset.VisitorId;
            preset.MemberId = request.MemberId ?? preset.MemberId;

            preset.UpdatedBy = username;
            preset.UpdatedAt = DateTime.UtcNow;

            if (request.Status.HasValue)
                preset.Status = request.Status.Value;

            var updated = await _repository.UpdateAsync(preset);
            return _mapper.Map<PresetDto>(updated);
        }

        // 6. DELETE PRESET
        public async Task<bool> DeleteAsync(Guid id)
        {
            var preset = await _repository.GetByIdAsync(id);
            if (preset == null)
                throw new NotFoundException($"Preset with ID {id} not found");

            SetDeleteAudit(preset);
            return await _repository.DeleteAsync(preset.Id);
        }
    }
}
