using System;

namespace Shared.Contracts.Read
{
    /// <summary>
    /// Read DTO for GroupBuildingAccess showing group and building relationship
    /// </summary>
    public class GroupBuildingAccessRead : BaseRead
    {
        public Guid GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? GroupLevelPriority { get; set; }
        public Guid BuildingId { get; set; }
        public string? BuildingName { get; set; }

        /// <summary>
        /// Number of members in this group
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// List of users in this group (optional, populated only when requested)
        /// </summary>
        public List<UserGroupMemberRead>? Members { get; set; }
    }
}
