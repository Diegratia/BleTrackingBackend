using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Helpers.Consumer;

namespace Repositories.Repository.RepoModel
{
    public class PatrolCaseRM : BaseModelDto
    {
        public Guid? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public CaseType? CaseType { get; set; }
        public CaseStatus? CaseStatus { get; set; }
        public Guid? PatrolSessionId { get; set; }
        public Guid? SecurityId { get; set; }
        public Guid? ApprovedByHeadId { get; set; }
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public DateTime? CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public MstSecurityLookUpRM? Security { get; set; }
        public PatrolAssignmentLookUpRM? PatrolAssignment { get; set; }
        public PatrolRouteMinimalRM? PatrolRoute { get; set; }
    }

    public class PatrolCaseFilter
{
    public string? Search { get; set; }

    public CaseType? CaseType { get; set; }
    public CaseStatus? CaseStatus { get; set; }

    public Guid? SecurityId { get; set; }
    public Guid? PatrolAssignmentId { get; set; }
    public Guid? PatrolRouteId { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

    
}