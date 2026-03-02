using System;

namespace Shared.Contracts
{
    public class MstAccessControlFilter : Shared.Contracts.Read.BaseFilter
    {
        public Guid? BrandId { get; set; }
        public Guid? IntegrationId { get; set; }
        public bool? IsAssigned { get; set; }
        public string? Type { get; set; }
    }
}
