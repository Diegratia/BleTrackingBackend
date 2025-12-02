using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Data.ViewModels.AlarmAnalytics;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingAnalyticsService : ITrackingAnalyticsService
    {
        private readonly TrackingAnalyticsRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingAnalyticsService> _logger;

        public TrackingAnalyticsService(
            TrackingAnalyticsRepository repository,
            IMapper mapper,
            ILogger<TrackingAnalyticsService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResponseCollection<TrackingAreaSummaryDto>> GetAreaSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetAreaSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingAreaSummaryDto>>(data);
                return ResponseCollection<TrackingAreaSummaryDto>.Ok(dto, "Area summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area summary");
                return ResponseCollection<TrackingAreaSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingDailySummaryDto>> GetDailySummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetDailySummaryAsync(request);
                var dto = _mapper.Map<List<TrackingDailySummaryDto>>(data);
                return ResponseCollection<TrackingDailySummaryDto>.Ok(dto, "daily summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident daily summary");
                return ResponseCollection<TrackingDailySummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingReaderSummaryDto>> GetReaderSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetReaderSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingReaderSummaryDto>>(data);
                return ResponseCollection<TrackingReaderSummaryDto>.Ok(dto, "status summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident status summary");
                return ResponseCollection<TrackingReaderSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingVisitorSummaryDto>> GetVisitorSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingVisitorSummaryDto>>(data);
                return ResponseCollection<TrackingVisitorSummaryDto>.Ok(dto, "Incident visitor summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident visitor summary");
                return ResponseCollection<TrackingVisitorSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingBuildingSummaryDto>> GetBuildingSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetBuildingSummaryAsync(request);
                var dto = _mapper.Map<List<TrackingBuildingSummaryDto>>(data);
                return ResponseCollection<TrackingBuildingSummaryDto>.Ok(dto, "Incident building summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident building summary");
                return ResponseCollection<TrackingBuildingSummaryDto>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingMovementRM>> GetTrackingMovementByCardIdAsync(Guid cardId)
        {
            try
            {
                var data = await _repository.GetTrackingMovementByCardIdAsync(cardId);
                return ResponseCollection<TrackingMovementRM>.Ok(data, "Tracking movement retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting tracking movement by card ID {CardId}", cardId);
                return ResponseCollection<TrackingMovementRM>.Error($"Internal server error: {ex.Message}");
            }
        }

        // ===========================================================
        // 🔹 Get Heatmap
        // ===========================================================
        public async Task<ResponseCollection<TrackingHeatmapRM>> GetHeatmapDataAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetHeatmapDataAsync(request);
                return ResponseCollection<TrackingHeatmapRM>.Ok(data, "Tracking heatmap retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting heatmap data");
                return ResponseCollection<TrackingHeatmapRM>.Error($"Internal server error: {ex.Message}");
            }
        }

        public async Task<ResponseCollection<TrackingCardSummaryRM>> GetCardSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetCardSummaryAsync(request);
                return ResponseCollection<TrackingCardSummaryRM>.Ok(data, "Tracking Summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting heatmap data");
                return ResponseCollection<TrackingCardSummaryRM>.Error($"Internal server error: {ex.Message}");
            }
        }
        
            public async Task<ResponseSingle<TrackingAccessPermissionSummaryDto>> GetAreaAccessedSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetAccessPermissionSummaryAsync(request);
                var dto = _mapper.Map<TrackingAccessPermissionSummaryDto>(data);
                return ResponseSingle<TrackingAccessPermissionSummaryDto>.Ok(dto, "Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting area count");
                return ResponseSingle<TrackingAccessPermissionSummaryDto>.Error($"Internal error: {ex.Message}");
            }
        }


    }
}
