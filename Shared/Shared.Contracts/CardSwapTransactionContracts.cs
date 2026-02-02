using System;
using System.Collections.Generic;
using Shared.Contracts;

namespace Shared.Contracts
{
    // DTO for creating swap transaction
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

    // DTO for updating swap transaction (minimal)
    public class CardSwapTransactionUpdateDto
    {
        public CardSwapStatus? CardSwapStatus { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    // Filter for DataTables
    public class CardSwapTransactionFilter
    {
        public string? Search { get; set; }
        public Guid? VisitorId { get; set; }
        public Guid? SwapChainId { get; set; }
        public SwapType? SwapType { get; set; }
        public CardSwapStatus? Status { get; set; }
        public Guid? MaskedAreaId { get; set; }
        public Guid? FromCardId { get; set; }
        public Guid? ToCardId { get; set; }
        
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
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

    public class CardSwapTransactionRead
    {
        public Guid Id { get; set; }
        public Guid FromCardId { get; set; }
        public string FromCardNumber { get; set; } = string.Empty;
        public Guid ToCardId { get; set; }
        public string ToCardNumber { get; set; } = string.Empty;
        public Guid VisitorId { get; set; }
        public string VisitorName { get; set; } = string.Empty;
        public Guid? TrxVisitorId { get; set; }
        public SwapType SwapType { get; set; }
        public CardSwapStatus CardSwapStatus { get; set; }
        public Guid MaskedAreaId { get; set; }
        public string MaskedAreaName { get; set; } = string.Empty;
        public Guid SwapChainId { get; set; }
        public int SwapSequence { get; set; }
        public string SwapBy { get; set; } = string.Empty;
        public DateTime? ExecutedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public IdentityType? IdentityType { get; set; }
        public string? IdentityValue { get; set; }
        public Guid ApplicationId { get; set; }
    }
}
