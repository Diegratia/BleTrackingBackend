using System;

namespace Shared.Contracts
{
    public class MemberFilter
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // Entity-specific filters
        public bool? IsBlacklist { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? CardNumber { get; set; }
        public string? PersonId { get; set; }
        public string? StatusEmployee { get; set; }
        public Guid? CardId { get; set; }  // For filtering by assigned card
    }
}
