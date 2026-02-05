using System;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class MstEngineFilter : BaseFilter
    {
        public int? Status { get; set; }
        public int? IsLive { get; set; }
    }
}
