using System;

namespace Shared.Contracts.Read
{
    public class UserRead : BaseRead
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public Guid GroupId { get; set; }
        public string? GroupName { get; set; }  // Include navigation
        public LevelPriority GroupLevel { get; set; }
        public int IsEmailConfirmation { get; set; }
        public bool IsIntegration { get; set; }
        public string? profilePicture { get; set; }
        public DateTime LastLoginAt { get; set; }
        public StatusEmployee StatusActive { get; set; }

        // Layer 2: Role Modifier dari Group
        public bool GroupIsHead { get; set; }

        // Layer 3: Permission override (nullable = inherit dari Group)
        public bool? CanApprovePatrol { get; set; }
        public bool? CanAlarmAction { get; set; }
        public bool? CanCreateMonitoringConfig { get; set; }
        public bool? CanUpdateMonitoringConfig { get; set; }

        // Effective value (resolved dari User override atau Group.IsHead)
        public bool EffectiveCanApprovePatrol { get; set; }
        public bool EffectiveCanAlarmAction { get; set; }
        public bool EffectiveCanCreateMonitoringConfig { get; set; }
        public bool EffectiveCanUpdateMonitoringConfig { get; set; }
    }
}
