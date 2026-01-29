using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Data.ViewModels.Dto.Helpers.MinimalDto;
using Helpers.Consumer.DtoHelpers;

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
        public string? StartAreaName { get; set; }
        public string? EndAreaName { get; set; }
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

    // public class PatrolAssignmentRM : BaseModelWithStatusRM
    // {

    //     public string? Name { get; set; }
    //     public string? Description { get; set; }
    //     public Guid? PatrolRouteId { get; set; }
    //     public Guid? TimeGroupId { get; set; }
    //     public DateTime? StartDate { get; set; }
    //     public DateTime? EndDate { get; set; }

    //     public PatrolRouteLookUpRM? PatrolRoute { get; set; }
    //     public AssignmentTimeGroupRM? TimeGroup { get; set; }
    //     public List<SecurityListRM>? Securities { get; set; }
    // }

    // public class SecurityListRM
    // {
    //     public Guid Id { get; set; }
    //     public string? Name { get; set; }
    //     public string? CardNumber { get; set; }
    //     public string? IdentityId { get; set; }
    //     public string? OrganizationName { get; set; }
    //     public string? DepartmentName { get; set; }
    //     public string? DistrictName { get; set; }
    // }

    // public class AssignmentTimeGroupRM
    // {
    //     public Guid Id { get; set; }
    //     public string? Name { get; set; }
    //     public string? ScheduleType { get; set; }
    //     public List<TimeBlockRM> TimeBlocks { get; set; } = new();
    // }

    // public class TimeBlockRM 
    // {
    //     public Guid Id { get; set;}
    //     [JsonConverter(typeof(JsonStringEnumConverter))]
    //     public DayOfWeek? DayOfWeek { get; set; }  
    //     public TimeSpan? StartTime { get; set; }
    //     public TimeSpan? EndTime { get; set; }
    // }

    
}