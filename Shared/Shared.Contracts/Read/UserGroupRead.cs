using System;
using System.Collections.Generic;

namespace Shared.Contracts.Read
{
    public class UserGroupRead : BaseRead
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }

        /// <summary>
        /// List of buildings accessible to this group
        /// </summary>
        public List<MstBuildingRead>? AccessibleBuildings { get; set; }

        /// <summary>
        /// List of users in this group (optional, for overview)
        /// </summary>
        public List<UserGroupMemberRead>? Members { get; set; }

        /// <summary>
        /// Count of members in this group
        /// </summary>
        public int MemberCount { get; set; }
    }

    /// <summary>
    /// Lightweight user info for group member list
    /// </summary>
    public class UserGroupMemberRead
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int Status { get; set; }
    }
}
