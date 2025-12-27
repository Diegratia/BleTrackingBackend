// File: BusinessLogic/Services/Implementation/Analytics/TrackingAnalyticsV2Service.cs
using AutoMapper;
using BusinessLogic.Services.Interface.Analytics;
using Data.ViewModels;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Repositories.Repository.RepoModel;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementation.Analytics
{
    public class TrackingAnalyticsV2Service : ITrackingAnalyticsV2Service
    {
        private readonly TrackingAnalyticsV2Repository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<TrackingAnalyticsV2Service> _logger;

        public TrackingAnalyticsV2Service(
            TrackingAnalyticsV2Repository repository,
            IMapper mapper,
            ILogger<TrackingAnalyticsV2Service> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

            public async Task<ResponseCollection<VisitorSessionSummaryRM>> GetVisitorSessionSummaryAsync(TrackingAnalyticsRequestRM request)
        {
            try
            {
                var data = await _repository.GetVisitorSessionSummaryAsync(request);
                return ResponseCollection<VisitorSessionSummaryRM>.Ok(data, "Visitor session summary retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVisitorSessionSummaryAsync");
                return ResponseCollection<VisitorSessionSummaryRM>.Error($"Internal error: {ex.Message}");
            }
        }

    }
}