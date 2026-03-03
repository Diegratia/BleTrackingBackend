using System;
using System.Text.Json;
using Shared.Contracts;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class CardFilter : BaseFilter
    {
        public JsonElement MemberId { get; set; }
        public JsonElement VisitorId { get; set; }
        public JsonElement SecurityId { get; set; }
        public JsonElement CardGroupId { get; set; }
        public string? CardNumber { get; set; }
        public string? Dmac { get; set; }
        public string? Name { get; set; }
        public CardType? CardType { get; set; }
        public CardStatus? CardStatus { get; set; }
        public bool? IsUsed { get; set; }
        public bool? IsMultiMaskedArea { get; set; }
        public Guid? RegisteredMaskedAreaId { get; set; }
        public int? StatusCard { get; set; }
    }
}
