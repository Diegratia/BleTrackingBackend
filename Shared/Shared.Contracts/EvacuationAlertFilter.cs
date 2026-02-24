using System.Text.Json;
using Shared.Contracts.Read;


namespace Shared.Contracts
{
    public class EvacuationAlertFilter : BaseFilter
    {
        public string? Search { get; set; }
        public JsonElement AlertStatus { get; set; }
        public JsonElement TriggerType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
