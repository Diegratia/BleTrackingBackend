using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Data.ViewModels
{
    public class CardSwapTransactionDto
    {
    }
    public class CardSwapTransactionCreateDto
    {
        public Guid VisitorId { get; set; }
        public Guid? TrxVisitorId { get; set; }
        public Guid FromCardId { get; set; }
        public Guid ToCardId { get; set; }
        public Guid MaskedAreaId { get; set; }
        public SwapType SwapType { get; set; }
        public SwapMode SwapMode { get; set; }
        public Guid SwapChainId { get; set; }
        public string SwapBy { get; set; } = string.Empty;
        public IdentityType? IdentityType { get; set; }
        public string? IdentityValue { get; set; }
    }

    public class ForwardSwapRequest
    {
        public Guid VisitorId { get; set; }
        public Guid? TrxVisitorId { get; set; }
        public Guid MaskedAreaId { get; set; }
        public Guid ToCardId { get; set; }
        public IdentityType? IdentityType { get; set; }
        public SwapMode? SwapMode { get; set; }
        public string? IdentityValue { get; set; }
        public string SwapBy { get; set; } = string.Empty;
    }
    
        public class ReverseSwapRequest
    {
        public Guid VisitorId { get; set; }
        public Guid SwapChainId { get; set; }
    }

}
