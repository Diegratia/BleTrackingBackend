using System;
using System.Collections.Generic;

namespace Shared.Contracts.Read
{
    /// <summary>
    /// Detailed user group info with members and building access
    /// </summary>
    public class UserGroupWithDetailsRead
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Role modifier: true = Head Security (bisa action + approval), false = Operator Biasa (view only)
        /// </summary>
        public bool IsHead { get; set; }

        /// <summary>
        /// List of buildings accessible to this group
        /// </summary>
        public List<MstBuildingRead> AccessibleBuildings { get; set; } = new();

        /// <summary>
        /// List of members in this group
        /// </summary>
        public List<UserGroupMemberRead> Members { get; set; } = new();

        /// <summary>
        /// Count of members
        /// </summary>
        public int MemberCount { get; set; }
        public int AccessibleBuildingCount { get; set; }

        /// <summary>
        /// Audit fields
        /// </summary>
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
