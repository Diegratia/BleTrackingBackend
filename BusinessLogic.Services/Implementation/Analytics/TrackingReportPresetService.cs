// File: BusinessLogic/Services/Implementation/Analytics/TrackingReportPresetService.cs
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Entities.Models;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingReportPresetService : ITrackingReportPresetService
    {
        private readonly TrackingReportPresetRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingReportPresetService> _logger;

        public TrackingReportPresetService(
            TrackingReportPresetRepository repository,
            IMapper mapper,
            ILogger<TrackingReportPresetService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        // 1. APPLY PRESET - Get analytics request from preset
        public async Task<TrackingAnalyticsRequestRM> ApplyPresetAsync(Guid presetId)
        {
            try
            {
                var preset = await _repository.GetByIdAsync(presetId);
                if (preset == null)
                    throw new KeyNotFoundException($"Preset with ID {presetId} not found");

                // Convert to analytics request
                return new TrackingAnalyticsRequestRM
                {
                    TimeRange = preset.TimeRange,
                    From = preset.TimeRange == "custom" ? preset.CustomFromDate : null,
                    To = preset.TimeRange == "custom" ? preset.CustomToDate : null,
                    ReportTitle = preset.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying preset: {PresetId}", presetId);
                throw;
            }
        }

        // 2. GET ALL PRESETS
        public async Task<List<PresetDto>> GetAllAsync()
        {
            try
            {
                var presets = await _repository.GetAllAsync();
                return _mapper.Map<List<PresetDto>>(presets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all presets");
                throw;
            }
        }

        // 3. GET BY ID
        public async Task<PresetDto> GetByIdAsync(Guid id)
        {
            try
            {
                var preset = await _repository.GetByIdAsync(id);
                return preset == null ? null : _mapper.Map<PresetDto>(preset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preset by ID: {Id}", id);
                throw;
            }
        }

        // 4. SAVE PRESET
        public async Task<PresetDto> SavePresetAsync(CreateCustomPresetRequest request)
        {
            try
            {
                var preset = new TrackingReportPreset
                {
                    Name = request.Name,
                    TimeRange = request.TimeRange,
                    CustomFromDate = request.TimeRange == "custom" ? request.FromDate : null,
                    CustomToDate = request.TimeRange == "custom" ? request.ToDate : null
                };

                var created = await _repository.CreateAsync(preset);
                return _mapper.Map<PresetDto>(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving preset");
                throw;
            }
        }

        // 5. UPDATE PRESET
        public async Task<PresetDto> UpdateAsync(Guid id, UpdatePresetRequest request)
        {
            try
            {
                var preset = await _repository.GetByIdAsync(id);
                if (preset == null)
                    throw new KeyNotFoundException($"Preset with ID {id} not found");

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Name))
                    preset.Name = request.Name;

                if (!string.IsNullOrEmpty(request.TimeRange))
                {
                    preset.TimeRange = request.TimeRange;
                    preset.CustomFromDate = request.TimeRange == "custom" ? request.FromDate : null;
                    preset.CustomToDate = request.TimeRange == "custom" ? request.ToDate : null;
                }

                if (request.Status.HasValue)
                    preset.Status = request.Status.Value;

                var updated = await _repository.UpdateAsync(preset);
                return _mapper.Map<PresetDto>(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preset: {Id}", id);
                throw;
            }
        }

        // 6. DELETE PRESET
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting preset: {Id}", id);
                throw;
            }
        }

        // 7. SEED SYSTEM PRESETS
        // public async Task SeedSystemPresetsAsync()
        // {
        //     try
        //     {
        //         await _repository.SeedSystemPresetsAsync();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error seeding system presets");
        //         throw;
        //     }
        // }
    }
}