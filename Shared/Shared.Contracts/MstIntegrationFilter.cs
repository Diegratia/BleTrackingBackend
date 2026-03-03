using System.Text.Json;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class MstIntegrationFilter : BaseFilter
    {
        public JsonElement BrandId { get; set; }
        public int? Status { get; set; }
    }
}
