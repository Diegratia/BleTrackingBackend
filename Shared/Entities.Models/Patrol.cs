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

     //PatrolCheckpointLog
    public class PatrolCheckpointLog : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_session_id")]
        public Guid? PatrolSessionId { get; set; }
        [Column("patrol_area_id")]
        public Guid? PatrolAreaId { get; set; }
        [Column("area_name_snapshot")]
        public string? AreaNameSnap { get; set; }
        [Column("order_index")]
        public int? OrderIndex { get; set; }
        [Column("min_dwell_time")]
        public int? MinDwellTime { get; set; }
        [Column("max_dwell_time")]
        public int? MaxDwellTime { get; set; }
        [Column("distance_from_prev_meters")]
        public double? DistanceFromPrevMeters { get; set; }
        [Column("arrived_at")]
        public DateTime? ArrivedAt { get; set; }
        [Column("left_at")]
        public DateTime? LeftAt { get; set; }
        [Column("cleared_at")]
        public DateTime? ClearedAt { get; set; }
        [Column("checkpoint_status")]
        public PatrolCheckpointStatus CheckpointStatus { get; set; } = PatrolCheckpointStatus.AutoDetected;
        [Column("notes")]
        public string? Notes { get; set; }

        public PatrolSession? PatrolSession { get; set; }
        public PatrolArea? PatrolArea { get; set; }
        public MstApplication? Application { get; set; }
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
        [Column("min_dwell_time")]
        public int? MinDwellTime { get; set; }
        [Column("max_dwell_time")]
        public int? MaxDwellTime { get; set; }
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
    //PatrolAssignment
    public class PatrolAssignment : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("name")]
        public string? Name { get; set; }
        [Column("description")]
        public string? Description { get; set; }
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }
        [Column("time_group_id")]
        public Guid? TimeGroupId { get; set; }
        [Column("approval_type")]
        public PatrolApprovalType ApprovalType { get; set; } = PatrolApprovalType.ByThreatLevel;
        [Column("start_date")]
        public DateTime? StartDate { get; set; }
        [Column("end_date")]
        public DateTime? EndDate { get; set; }
        [Column("duration_type")]
        public PatrolDurationType DurationType { get; set; } = PatrolDurationType.WithDuration;
        [Column("start_type")]
        public PatrolStartType StartType { get; set; } = PatrolStartType.Manual;
        [Column("cycle_count")]
        public int CycleCount { get; set; } = 1;
        [Column("cycle_type")]
        public PatrolCycleType CycleType { get; set; } = PatrolCycleType.HalfCycle;
        [Column("security_head_1")]
        public Guid? SecurityHead1Id { get; set; }
        [Column("security_head_2")]
        public Guid? SecurityHead2Id { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public TimeGroup? TimeGroup { get; set; }
        public MstApplication? Application { get; set; }
        public MstSecurity? SecurityHead1 { get; set; }
        public MstSecurity? SecurityHead2 { get; set; }
        public ICollection<PatrolAssignmentSecurity> PatrolAssignmentSecurities { get; set; } = new List<PatrolAssignmentSecurity>();
        public ICollection<PatrolSession> PatrolSessions { get; set; } = new List<PatrolSession>();
        public ICollection<PatrolShiftReplacement> PatrolShiftReplacements { get; set; } = new List<PatrolShiftReplacement>();
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
        [Column("threat_level")]
        public ThreatLevel? ThreatLevel { get; set; }
        [Column("patrol_area_id")]
        public Guid? PatrolAreaId { get; set; }
        [Column("patrol_area_name_snap")]
        public string? PatrolAreaNameSnap { get; set; }
        [Column("patrol_session_id")]
        public Guid? PatrolSessionId { get; set; }
        [Column("security_id")]
        public Guid? SecurityId { get; set; }
        [Column("security_head_1")]
        public Guid? SecurityHead1Id { get; set; } //snapshot
        [Column("security_head_2")]
        public Guid? SecurityHead2Id { get; set; } //snapshot
        [Column("approval_type")]
        public PatrolApprovalType ApprovalType { get; set; }
        [Column("approved_by_head_1_id")]
        public Guid? ApprovedByHead1Id { get; set; }
        [Column("approved_by_head_2_id")]
        public Guid? ApprovedByHead2Id { get; set; }
        [Column("approved_by_head_1_at")]
        public DateTime? ApprovedByHead1At { get; set; }
        [Column("approved_by_head_2_at")]
        public DateTime? ApprovedByHead2At { get; set; }
        [Column("patrol_assignment_id")]
        public Guid? PatrolAssignmentId { get; set; } // snapshot dari assignment
        [Column("patrol_route_id")]
        public Guid? PatrolRouteId { get; set; }    // snapshot dari assignment
        public PatrolAssignment? PatrolAssignment { get; set; }
        public MstSecurity? Security { get; set; }
        public MstSecurity? SecurityHead1 { get; set; }
        public MstSecurity? SecurityHead2 { get; set; }
        public MstSecurity? ApprovedByHead1 { get; set; }
        public MstSecurity? ApprovedByHead2 { get; set; }
        public PatrolArea? PatrolArea { get; set; }
        public PatrolSession? PatrolSession { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolCaseAttachment> PatrolCaseAttachments { get; set; } = new List<PatrolCaseAttachment>();
    }
    //PatrolSession - ini merupakan snapshot
    public class PatrolSession : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_route_id")]
        public Guid PatrolRouteId { get; set; } 
        [Column("patrol_route_name_snap")]
        public string? PatrolRouteNameSnap { get; set; }
        [Column("patrol_assignment_id")]
        public Guid PatrolAssignmentId { get; set; } 
        [Column("patrol_assignment_name_snap")]
        public string? PatrolAssignmentNameSnap { get; set; }
        [Column("security_id")]
        public Guid SecurityId { get; set; } 
        [Column("security_name_snap")]
        public string? SecurityNameSnap { get; set; }
        [Column("security_identity_id_snap")]
        public string? SecurityIdentityIdSnap { get; set; }
        [Column("security_card_number_snap")]
        public string? SecurityCardNumberSnap { get; set; }
        [Column("time_group_id")]
        public Guid? TimeGroupId { get; set; }
        [Column("time_group_name_snap")]
        public string? TimeGroupNameSnap { get; set; }
        [Column("started_at")]
        public DateTime StartedAt { get; set; } // event action
        [Column("ended_at")]
        public DateTime? EndedAt { get; set; }
        public PatrolAssignment? PatrolAssignment { get; set; }
        public PatrolRoute? PatrolRoute { get; set; }
        public MstSecurity? Security { get; set; }
        public TimeGroup? TimeGroup { get; set; }
        public MstApplication? Application { get; set; }
        public ICollection<PatrolCase> PatrolCases { get; set; } = new List<PatrolCase>();
        public ICollection<PatrolCheckpointLog> PatrolCheckpointLogs { get; set; } = new List<PatrolCheckpointLog>();
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

    //PatrolShiftReplacement
    public class PatrolShiftReplacement : BaseModelWithTimeApp, IApplicationEntity
    {
        [Column("patrol_assignment_id")]
        public Guid PatrolAssignmentId { get; set; }
        
        [Column("original_security_id")]
        public Guid OriginalSecurityId { get; set; }
        
        [Column("substitute_security_id")]
        public Guid SubstituteSecurityId { get; set; }
        
        [Column("replacement_start_date")]
        public DateTime ReplacementStartDate { get; set; }
        
        [Column("replacement_end_date")]
        public DateTime ReplacementEndDate { get; set; }
        
        [Column("reason")]
        public string? Reason { get; set; }
        
        public PatrolAssignment? PatrolAssignment { get; set; }
        public MstSecurity? OriginalSecurity { get; set; }
        public MstSecurity? SubstituteSecurity { get; set; }
        public MstApplication? Application { get; set; }
    }
}
