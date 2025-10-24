using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class AlarmAnalyticsIncidentService : IAlarmAnalyticsIncidentService
    {
        private readonly AlarmAnalyticsIncidentRepository _repository;
        private readonly ILogger<AlarmAnalyticsIncidentService> _logger;

        public AlarmAnalyticsIncidentService(
            AlarmAnalyticsIncidentRepository repository,
            ILogger<AlarmAnalyticsIncidentService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ===================================================================
        // 1️⃣ Area Summary (Incident-level)
        // ===================================================================
        public async Task<ResponseCollection<object>> GetAreaSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetAreaSummaryAsync(request);
                return ResponseCollection<object>.Ok(data.Cast<object>(), "Incident area summary retrieved Okfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident area summary");
                return ResponseCollection<object>.Error($"Internal server error: {ex.Message}");
            }
        }

        // ===================================================================
        // 2️⃣ Daily Summary (Incident-level)
        // ===================================================================
        public async Task<ResponseCollection<object>> GetDailySummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetDailySummaryAsync(request);
                return ResponseCollection<object>.Ok(data.Cast<object>(), "Incident area summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident daily summary");
                return ResponseCollection<object>.Error($"Internal server error: {ex.Message}");
            }
        }

        // ===================================================================
        // 3️⃣ Status Summary (Incident-level)
        // ===================================================================
        public async Task<ResponseCollection<object>> GetStatusSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetStatusSummaryAsync(request);
                return ResponseCollection<object>.Ok(data.Cast<object>(), "Incident status summary retrieved Okfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident status summary");
                return ResponseCollection<object>.Error($"Internal server error: {ex.Message}");
            }
        }

        // ===================================================================
        // 4️⃣ Visitor Summary (Incident-level)
        // ===================================================================
        public async Task<ResponseCollection<object>> GetVisitorSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSummaryAsync(request);
                return ResponseCollection<object>.Ok(data.Cast<object>(), "Incident visitor summary retrieved Okfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident visitor summary");
                return ResponseCollection<object>.Error($"Internal server error: {ex.Message}");
            }
        }

        // ===================================================================
        // 5️⃣ Building Summary (Incident-level)
        // ===================================================================
        public async Task<ResponseCollection<object>> GetBuildingSummaryAsync(AlarmAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetBuildingSummaryAsync(request);
                return ResponseCollection<object>.Ok(data.Cast<object>(), "Incident building summary retrieved Okfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident building summary");
                return ResponseCollection<object>.Error($"Internal server error: {ex.Message}");
            }
        }
    }
}
