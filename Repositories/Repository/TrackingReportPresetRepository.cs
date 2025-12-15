// File: Repositories/Repository/Analytics/TrackingReportPresetRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.DbContexts;

namespace Repositories.Repository.Analytics
{
    public class TrackingReportPresetRepository : BaseRepository
    {
        public TrackingReportPresetRepository(
            BleTrackingDbContext context, 
            IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        {
        }

        // Get by ID
        public async Task<TrackingReportPreset> GetByIdAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingReportPresets
                .Where(p => p.Id == id && p.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.FirstOrDefaultAsync();
        }

        // Get all
        public async Task<List<TrackingReportPreset>> GetAllAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingReportPresets
                .Where(p => p.Status != 0);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        // Create
        public async Task<TrackingReportPreset> CreateAsync(TrackingReportPreset preset)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            if (!isSystemAdmin && applicationId.HasValue)
            {
                // Jika perlu set ApplicationId
                // preset.ApplicationId = applicationId.Value;
            }

            _context.TrackingReportPresets.Add(preset);
            await _context.SaveChangesAsync();
            return preset;
        }

        // Update
        public async Task<TrackingReportPreset> UpdateAsync(TrackingReportPreset preset)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            // Validasi user bisa update preset ini
            ValidateApplicationIdForEntity(preset, applicationId, isSystemAdmin);

            preset.UpdatedAt = DateTime.UtcNow;
            _context.TrackingReportPresets.Update(preset);
            await _context.SaveChangesAsync();
            return preset;
        }

        // Soft delete
        public async Task<bool> DeleteAsync(Guid id)
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingReportPresets
                .Where(p => p.Id == id && p.Status == 1);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            var preset = await query.FirstOrDefaultAsync();
            if (preset == null) return false;

            preset.Status = 1;
            preset.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Get all for export
        public async Task<List<TrackingReportPreset>> GetAllExportAsync()
        {
            var (applicationId, isSystemAdmin) = GetApplicationIdAndRole();

            var query = _context.TrackingReportPresets
                .Where(p => p.Status == 1);

            query = ApplyApplicationIdFilter(query, applicationId, isSystemAdmin);

            return await query.ToListAsync();
        }

        // Seed system presets
        // public async Task SeedSystemPresetsAsync()
        // {
        //     if (!await _context.TrackingReportPresets.AnyAsync())
        //     {
        //         var systemPresets = new List<TrackingReportPreset>
        //         {
        //             new TrackingReportPreset
        //             {
        //                 Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        //                 Name = "Daily Visitor Report",
        //                 TimeRange = "daily",
        //                 CreatedAt = DateTime.UtcNow,
        //                 IsActive = true
        //             },
        //             new TrackingReportPreset
        //             {
        //                 Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        //                 Name = "Weekly Summary",
        //                 TimeRange = "weekly",
        //                 CreatedAt = DateTime.UtcNow,
        //                 IsActive = true
        //             },
        //             new TrackingReportPreset
        //             {
        //                 Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        //                 Name = "Monthly All Areas",
        //                 TimeRange = "monthly",
        //                 CreatedAt = DateTime.UtcNow,
        //                 IsActive = true
        //             }
        //         };

        //         await _context.TrackingReportPresets.AddRangeAsync(systemPresets);
        //         await _context.SaveChangesAsync();
        //     }
        // }
    }
}