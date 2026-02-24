using System.Text.Json;
using Shared.Contracts.Read;


namespace Shared.Contracts
{
    public class EvacuationTransactionFilter : BaseFilter
    {
        public string? Search { get; set; }
        public JsonElement EvacuationAlertId { get; set; }
        public JsonElement EvacuationAssemblyPointId { get; set; }
        public JsonElement PersonCategory { get; set; }
        public JsonElement PersonStatus { get; set; }
    }
}
