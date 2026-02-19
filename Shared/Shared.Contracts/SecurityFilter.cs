using System;
using Shared.Contracts.Read;

namespace Shared.Contracts
{
    public class SecurityFilter : BaseFilter
    {
        public string? Search { get; set; }
        public bool? IsHead { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? CardNumber { get; set; }
    }
}
