using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.ViewModels
{
    public class PatrolAssignmentDto : BaseModelWithStatusDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public string? PatrolRouteName { get; set; }
        // public List<Guid>? SecurityIds { get; set; }
        public Guid? TimeGroupId { get; set; }
        public string? TimeGroupName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<SecurityListDto>? Securities { get; set; }
    }

    public class PatrolAssignmentCreateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public List<Guid>? SecurityIds { get; set; }
        public Guid? TimeGroupId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PatrolAssignmentUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid? PatrolRouteId { get; set; }
        public List<Guid>? SecurityIds { get; set; }
        public Guid? TimeGroupId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PatrolAssignmentLookUpDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
    }

    public class SecurityListDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? IdentityId { get; set; }
        public string? OrganizationName { get; set; }
        public string? DepartmentName { get; set; }
        public string? DistrictName { get; set; }
    }

    

}