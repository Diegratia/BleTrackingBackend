using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Shared.Contracts;


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
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public MstSecurityLookUpRM? Security { get; set; }
        public PatrolAssignmentLookUpRM? PatrolAssignment { get; set; }
        public PatrolRouteMinimalRM? PatrolRoute { get; set; }
    }
}