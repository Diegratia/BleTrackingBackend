using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Shared.Contracts.Analytics;
using BusinessLogic.Services.Interface.Analytics;
using BusinessLogic.Services.Interface;

namespace BusinessLogic.Services.Implementation.Analytics
{
    /// <summary>
    /// Implementation of User Journey Analytics Service
    /// Provides methods for common paths analysis and journey replay
    /// </summary>
    public class UserJourneyService : IUserJourneyService
    {
        private readonly UserJourneyRepository _repository;
        private readonly ILogger<UserJourneyService> _logger;
        private readonly IAuditEmitter _audit;

        public UserJourneyService(
            UserJourneyRepository repository,
            ILogger<UserJourneyService> logger,
            IAuditEmitter audit)
        {
            _repository = repository;
            _logger = logger;
            _audit = audit;
        }

        /// <summary>
        /// Get common paths analysis - most popular journey sequences
        /// Direct return from repository (no mapper needed as repository returns Read DTO)
        /// </summary>
        public async Task<CommonPathsResponse> GetCommonPathsAsync(UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Getting common paths analysis");

                var result = await _repository.GetCommonPathsAsync(filter);

                _logger.LogInformation(
                    "Retrieved {Count} common paths, total journeys: {TotalJourneys}",
                    result.Data.Count,
                    result.TotalJourneys);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting common paths analysis");
                throw;
            }
        }

        /// <summary>
        /// Get journey replay with incident markers for a specific visitor
        /// Direct return from repository (no mapper needed as repository returns Read DTO)
        /// </summary>
        public async Task<JourneyReplayRead> GetJourneyReplayForVisitorAsync(
            Guid visitorId,
            UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Getting journey replay for visitor {VisitorId}", visitorId);

                var fromUtc = filter.From ?? DateTime.UtcNow.AddDays(-1);
                var toUtc = filter.To ?? DateTime.UtcNow;

                var result = await _repository.GetJourneyReplayAsync(
                    visitorId,
                    null,
                    fromUtc,
                    toUtc);

                _logger.LogInformation(
                    "Journey replay completed for visitor {VisitorId}: {StepCount} steps, {IncidentCount} incidents",
                    visitorId,
                    result.JourneySteps.Count,
                    result.TotalIncidents);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting journey replay for visitor {VisitorId}", visitorId);
                throw;
            }
        }

        /// <summary>
        /// Get journey replay with incident markers for a specific member
        /// Direct return from repository (no mapper needed as repository returns Read DTO)
        /// </summary>
        public async Task<JourneyReplayRead> GetJourneyReplayForMemberAsync(
            Guid memberId,
            UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Getting journey replay for member {MemberId}", memberId);

                var fromUtc = filter.From ?? DateTime.UtcNow.AddDays(-1);
                var toUtc = filter.To ?? DateTime.UtcNow;

                var result = await _repository.GetJourneyReplayAsync(
                    null,
                    memberId,
                    fromUtc,
                    toUtc);

                _logger.LogInformation(
                    "Journey replay completed for member {MemberId}: {StepCount} steps, {IncidentCount} incidents",
                    memberId,
                    result.JourneySteps.Count,
                    result.TotalIncidents);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting journey replay for member {MemberId}", memberId);
                throw;
            }
        }
    }
}
