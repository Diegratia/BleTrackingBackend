using System;
using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class MonitoringConfigFilter : BaseFilter
    {
        public string? Name { get; set; }
        public JsonElement ApplicationId { get; set; }  // Supports both single and array

        /// <summary>
        /// Filter by building ID (supports single GUID or array of GUIDs)
        /// </summary>
        public JsonElement BuildingId { get; set; }
    }
}
