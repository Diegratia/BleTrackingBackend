using System;
using System.Threading.Tasks;
using Shared.Contracts.Analytics;

namespace BusinessLogic.Services.Interface.Analytics
{
    /// <summary>
    /// Interface for User Journey Analytics Service
    /// Provides methods for common paths analysis and security checks
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
        /// Perform security journey check for a specific visitor
        /// </summary>
        /// <param name="visitorId">Visitor ID to check</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Security check results with violations and risk level</returns>
        Task<SecurityJourneyCheckRead> GetSecurityCheckForVisitorAsync(Guid visitorId, UserJourneyFilter filter);

        /// <summary>
        /// Perform security journey check for a specific member
        /// </summary>
        /// <param name="memberId">Member ID to check</param>
        /// <param name="filter">Filter criteria including date range</param>
        /// <returns>Security check results with violations and risk level</returns>
        Task<SecurityJourneyCheckRead> GetSecurityCheckForMemberAsync(Guid memberId, UserJourneyFilter filter);
    }
}
