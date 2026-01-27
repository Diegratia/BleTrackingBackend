using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;

namespace Repositories.Repository.RepoModel
{
    public class PatrolRouteRM
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<PatrolRouteAreaDtoRM> Areas { get; set; } = new();
        // public List<Guid?> PatrolAreaIds { get; set; } = new();
        // public float EstimatedDistance { get; set; }
        // public int EstimatedTime  { get; set; }
        // public Guid? StartAreaId { get; set; }
        // public Guid? EndAreaId { get; set; }
    }


    public class PatrolRouteLookUpRM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<Guid?> PatrolAreaIds { get; set; } = new();
        public List<Guid?> TimeGroupIds { get; set; } = new();
    }

    public class PatrolRouteAreaDtoRM
    {
        public Guid PatrolAreaId { get; set; }
        public int OrderIndex { get; set; }

        public float EstimatedDistance { get; set; }
        public int EstimatedTime { get; set; }

        public Guid? StartAreaId { get; set; }
        public Guid? EndAreaId { get; set; }
    }

    public class PatrolAssignmentLookUpRM
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Guid ApplicationId { get; set; }
    }

    public class PatrolRouteMinimalRM : BaseModelDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    
}