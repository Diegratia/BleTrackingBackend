using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class MstApplicationFilter : BaseFilter
    {
        public int? ApplicationStatus { get; set; }
    }
}
