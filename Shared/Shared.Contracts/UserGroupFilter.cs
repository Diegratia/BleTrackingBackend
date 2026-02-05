using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class UserGroupFilter : BaseFilter
    {
        public string? Name { get; set; }
        public string? LevelPriority { get; set; }
        public JsonElement ApplicationId { get; set; }  // Supports both single and array

        /// <summary>
        /// Filter by member IDs - groups that contain these specific members (supports single GUID or array of GUIDs)
        /// </summary>
        public JsonElement MemberId { get; set; }
    }
}
