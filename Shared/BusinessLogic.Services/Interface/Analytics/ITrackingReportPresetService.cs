// File: BusinessLogic/Services/Interface/Analytics/ITrackingReportPresetService.cs
using Data.ViewModels;
using Repositories.Repository.RepoModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interface.Analytics
{
    public interface ITrackingReportPresetService
    {
        // Apply preset untuk visitor session
        Task<TrackingAnalyticsRequestRM> ApplyPresetAsync(Guid presetId);
        
        // Basic CRUD
        Task<List<PresetDto>> GetAllAsync();
        Task<PresetDto> GetByIdAsync(Guid id);
        Task<PresetDto> SavePresetAsync(CreateCustomPresetRequest request);
        Task<PresetDto> UpdateAsync(Guid id, UpdatePresetRequest request);
        Task<bool> DeleteAsync(Guid id);
        
        // Seed system presets
        // Task SeedSystemPresetsAsync();
    }
}