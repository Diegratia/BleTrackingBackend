using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Helpers.Consumer;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Entities.Models
{
    public class PatrolArea : BaseModelWithTime, IApplicationEntity
    {

        [Column("name")]
        public string? Name { get; set; }

        [Column("area_shape")]
        public string? AreaShape { get; set; }
        [Column("color")]
        public string? Color { get; set; }
        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("floorplan_id")]
        public Guid? FloorplanId { get; set; }

        [Column("floor_id")]
        public Guid? FloorId { get; set; }

        [Column("is_active")]
        public int IsActive { get; set; } = 1;

        [Column("status")]
        public int Status { get; set; } = 1;

        [Column("application_id")]
        public Guid ApplicationId { get; set; }
        public MstApplication Application { get; set; }
        public MstFloor Floor { get; set; } //MstFloor
        public MstFloorplan Floorplan { get; set; }
        public ICollection<PatrolRouteAreas> PatrolRouteAreas { get; set; } = new List<PatrolRouteAreas>();
    }
    //PatrolRoute
    public class PatrolRoute : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("status")]
        public int Status { get; set; } = 1;
        public MstApplication Application { get; set; }
        public ICollection<PatrolRouteAreas> PatrolRouteAreas { get; set; } = new List<PatrolRouteAreas>();
        public ICollection<PatrolRouteTimeGroups> PatrolRouteTimeGroups { get; set; } = new List<PatrolRouteTimeGroups>();
        public ICollection<PatrolAssignment> PatrolAssignments { get; set; } = new List<PatrolAssignment>();
        public ICollection<PatrolCase> PatrolCases { get; set; } = new List<PatrolCase>();
        public ICollection<PatrolSession> PatrolSessions { get; set; } = new List<PatrolSession>();

    }
    //PatrolRouteAreas
    [Index(nameof(PatrolRouteId))]
    [Index(nameof(PatrolAreaId))]
    public class PatrolRouteAreas : BaseModelOnlyIdWithTime, IApplicationEntity
    {
        [Column("patrol_route_id")]
        public Guid PatrolRouteId { get; set; }
        [Column("patrol_area_id")]
        public Guid PatrolAreaId { get; set; }
        [Column("order_index")]
        public int OrderIndex { get; set; }
        [Column("estimated_distance")]
        public float EstimatedDistance { get; set; }
        [Column("estimated_time")]
        public int EstimatedTime { get; set; }
        [Column("start_area_id")]
        public Guid? StartAreaId { get; set; }
        [Column("end_area_id")]
        public Guid? EndAreaId { get; set; }
        [Column("status")]
        public int status { get; set; } = 1;
        public PatrolRoute PatrolRoute { get; set; }
        public PatrolArea PatrolArea { get; set; }
        public MstApplication Application { get; set; }
    }
    //PatrolRouteTimeGroups
    [Table("patrol_route_time_groups")]
    public class PatrolRouteTimeGroups : IApplicationEntity
    {
        [Column("patrol_route_id")]
        public Guid PatrolRouteId { get; set; }
        public PatrolRoute? PatrolRoutes { get; set; }

        [Column("time_group_id")]
        public Guid TimeGroupId { get; set; }
        public TimeGroup? TimeGroup { get; set; }

        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

    }
    //PatrolAssignment
    public class PatrolAssignment : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolAssignmentSecurity> PatrolAssignmentSecurities { get; set; } = new List<PatrolAssignmentSecurity>();
        public ICollection<PatrolSession> PatrolSessions { get; set; } = new List<PatrolSession>();
    }
    //PatrolAssignmentSecurity
    public class PatrolAssignmentSecurity : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_assignment_id")]
        public Guid PatrolAssignmentId { get; set; }
        [Column("security_id")]
        public Guid SecurityId { get; set; }
        public PatrolAssignment? PatrolAssignment { get; set; }
        public MstSecurity? Security { get; set; }
        public MstApplication? Application { get; set; }
    }
    //PatrolCase
    public class PatrolCase : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("title")]
        public string? Title { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("case_type")]
        public CaseType CaseType { get; set; }
        [Column("case_status")]
        public CaseStatus CaseStatus { get; set; }
        [Column("patrol_session_id")]
        public Guid? PatrolSessionId { get; set; }
        [Column("security_id")]
        public Guid? SecurityId { get; set; }
        [Column("approved_by_head_id")]
        public Guid? ApprovedByHeadId { get; set; }
        [Column("patrol_assignment_id")]
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public PatrolAssignment? PatrolAssignment { get; set; }
        public MstSecurity? Security { get; set; }
        public MstSecurity? ApprovedByHead { get; set; }
        public PatrolSession? PatrolSession { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolCaseAttachment> PatrolCaseAttachments { get; set; } = new List<PatrolCaseAttachment>();
    }
    //PatrolSession
    public class PatrolSession : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_route_id")]
        public Guid PatrolRouteId { get; set; } // snapshot dari engine
        [Column("patrol_assignment_id")]
        public Guid PatrolAssignmentId { get; set; } // snapshot dari engine
        [Column("security_id")]
        public Guid SecurityId { get; set; } // snapshot dari engine
        [Column("started_at")]
        public DateTime StartedAt { get; set; } // event action
        [Column("ended_at")]
        public DateTime? EndedAt { get; set; }
        public PatrolAssignment? PatrolAssignment { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstSecurity? Security { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolCase> PatrolCases { get; set; } = new List<PatrolCase>();
        public ICollection<PatrolCheckpointLog> PatrolCheckpointLogs { get; set; } = new List<PatrolCheckpointLog>();
    }
    //PatrolCheckpointLog
    public class PatrolCheckpointLog : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_session_id")]
        public Guid? PatrolSessionId { get; set; }
        [Column("patrol_area_id")]
        public Guid? PatrolAreaId { get; set; }
        [Column("order_index")]
        public int? OrderIndex { get; set; }
        [Column("arrived_at")]
        public DateTime? ArrivedAt { get; set; }
        [Column("left_at")]
        public DateTime? LeftAt { get; set; }
        [Column("distance_from_prev")]
        public DateTime? DistanceFromPrev { get; set; }
        public PatrolSession? PatrolSession { get; set; }
        public PatrolArea? PatrolArea { get; set; }
        public MstApplication? Application { get; set; }
    }
    //PatrolCaseAttachment
    public class PatrolCaseAttachment : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_case_id")]
        public Guid? PatrolCaseId { get; set; }
        [Column("file_url")]
        public string? FileUrl { get; set; }
        [Column("file_type")]
        public FileType? FileType { get; set; }
        [Column("mime_type")]
        public string? MimeType { get; set; }
        [Column("uploaded_at")]
        public DateTime? UploadedAt { get; set; }
        public PatrolCase? PatrolCase { get; set; }
        public MstApplication? Application { get; set; }
    }
}