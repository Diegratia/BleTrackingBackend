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
    }
}
