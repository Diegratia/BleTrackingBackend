using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Repositories.Repository.Analytics;
using Shared.Contracts.Analytics;
using BusinessLogic.Services.Interface.Analytics;

namespace BusinessLogic.Services.Implementation.Analytics
{
    /// <summary>
    /// Implementation of User Journey Analytics Service
    /// Provides methods for common paths analysis, security checks, and next area prediction
    /// </summary>
    public class UserJourneyService : IUserJourneyService
    {
        private readonly UserJourneyRepository _repository;
        private readonly ILogger<UserJourneyService> _logger;

        public UserJourneyService(
            UserJourneyRepository repository,
            ILogger<UserJourneyService> logger)
        {
            _repository = repository;
            _logger = logger;
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
        /// Perform security journey check for a specific visitor
        /// Direct return from repository (no mapper needed as repository returns Read DTO)
        /// </summary>
        public async Task<SecurityJourneyCheckRead> GetSecurityCheckForVisitorAsync(
            Guid visitorId,
            UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Performing security check for visitor {VisitorId}", visitorId);

                var fromUtc = filter.From ?? DateTime.UtcNow.AddDays(-1);
                var toUtc = filter.To ?? DateTime.UtcNow;

                var result = await _repository.GetSecurityCheckAsync(
                    visitorId,
                    null,
                    fromUtc,
                    toUtc);

                _logger.LogInformation(
                    "Security check completed for visitor {VisitorId}: {RiskLevel} risk, {ViolationCount} violations",
                    visitorId,
                    result.RiskLevel,
                    result.Violations.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing security check for visitor {VisitorId}", visitorId);
                throw;
            }
        }

        /// <summary>
        /// Perform security journey check for a specific member
        /// Direct return from repository (no mapper needed as repository returns Read DTO)
        /// </summary>
        public async Task<SecurityJourneyCheckRead> GetSecurityCheckForMemberAsync(
            Guid memberId,
            UserJourneyFilter filter)
        {
            try
            {
                _logger.LogInformation("Performing security check for member {MemberId}", memberId);

                var fromUtc = filter.From ?? DateTime.UtcNow.AddDays(-1);
                var toUtc = filter.To ?? DateTime.UtcNow;

                var result = await _repository.GetSecurityCheckAsync(
                    null,
                    memberId,
                    fromUtc,
                    toUtc);

                _logger.LogInformation(
                    "Security check completed for member {MemberId}: {RiskLevel} risk, {ViolationCount} violations",
                    memberId,
                    result.RiskLevel,
                    result.Violations.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing security check for member {MemberId}", memberId);
                throw;
            }
        }
    }
}
