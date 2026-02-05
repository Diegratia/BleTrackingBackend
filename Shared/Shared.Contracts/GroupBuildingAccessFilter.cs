using System.Text.Json;

namespace Shared.Contracts
{
    public class GroupBuildingAccessFilter : Shared.Contracts.Read.BaseFilter
    {
        public JsonElement GroupId { get; set; }
        public JsonElement BuildingId { get; set; }
    }
}
