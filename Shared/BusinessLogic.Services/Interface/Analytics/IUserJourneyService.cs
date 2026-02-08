using System;
using System.Threading.Tasks;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics
{
    /// <summary>
    /// Interface for User Journey Analytics Service
    /// Provides methods for common paths analysis and journey replay
    /// </summary>
    public interface IUserJourneyService
    {
        /// <summary>
        /// Get common paths analysis - most popular journey sequences
        /// </summary>
        /// <param name="filter">Filter criteria including date range, building, floor, etc.</param>
        /// <returns>Common paths with journey counts, percentages, and risk levels</returns>
        Task<CommonPathsResponse> GetCommonPathsAsync(UserJourneyFilter filter);

        /// <summary>
        /// Get journey replay with incident markers for a specific visitor
        /// Shows the path taken with areas where incidents occurred marked
        /// </summary>
        /// <param name="visitorId">Visitor ID to replay journey</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Journey replay with incident markers and statistics</returns>
        Task<JourneyReplayRead> GetJourneyReplayForVisitorAsync(Guid visitorId, UserJourneyFilter filter);

        /// <summary>
        /// Get journey replay with incident markers for a specific member
        /// Shows the path taken with areas where incidents occurred marked
        /// </summary>
        /// <param name="memberId">Member ID to replay journey</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Journey replay with incident markers and statistics</returns>
        Task<JourneyReplayRead> GetJourneyReplayForMemberAsync(Guid memberId, UserJourneyFilter filter);
    }
}
