using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class EvacuationAssemblyPointFilter : BaseFilter
    {
        public string? Search { get; set; }
        public JsonElement FloorId { get; set; }
        public JsonElement FloorplanId { get; set; }
        public JsonElement FloorplanMaskedAreaId { get; set; }
        public int? IsActive { get; set; }
    }
}
