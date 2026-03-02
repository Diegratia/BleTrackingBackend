using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class EvacuationAssemblyPointFilter : BaseFilter
    {
        public new string? Search { get; set; }
        public JsonElement FloorId { get; set; }
        public JsonElement FloorplanId { get; set; }
        public int? IsActive { get; set; }
    }
}
