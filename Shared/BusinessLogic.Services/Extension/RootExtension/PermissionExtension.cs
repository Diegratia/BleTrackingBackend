using Shared.Contracts;
using Shared.Contracts.Read;

namespace BusinessLogic.Services.Extension.RootExtension
{
    /// <summary>
    /// Permission extension methods for checking user access based on Layer 1 + 2 + 3 permission system
    /// </summary>
    public static class PermissionExtension
    {
        /// <summary>
        /// Cek apakah user bisa approve patrol (Layer 1 + 2 + 3)
        /// </summary>
        /// <param name="user">User to check permission for</param>
        /// <returns>true jika user bisa approve patrol, false otherwise</returns>
        public static bool HasPatrolApprovalPermission(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.GroupLevel <= LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.GroupLevel != LevelPriority.PrimaryAdmin)
                return false;

            // Layer 2 + 3: Harus IsHead DAN CanApprovePatrol true
            // null = inherit dari IsHead, jadi true = true, false = false
            return user.EffectiveCanApprovePatrol;
        }

        /// <summary>
        /// Cek apakah user bisa alarm action (Layer 1 + 3)
        /// </summary>
        /// <param name="user">User to check permission for</param>
        /// <returns>true jika user bisa alarm action, false otherwise</returns>
        public static bool HasAlarmActionPermission(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.GroupLevel <= LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.GroupLevel != LevelPriority.PrimaryAdmin)
                return false;

            // Layer 2 + 3: Harus IsHead DAN CanAlarmAction true
            return user.EffectiveCanAlarmAction;
        }

        /// <summary>
        /// Cek apakah user bisa create monitoring config (Layer 1 + 2 + 3)
        /// </summary>
        /// <param name="user">User to check permission for</param>
        /// <returns>true jika user bisa create monitoring config, false otherwise</returns>
        public static bool HasCreateMonitoringConfigPermission(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.GroupLevel <= LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.GroupLevel != LevelPriority.PrimaryAdmin)
                return false;

            // Layer 2 + 3: Harus IsHead DAN CanCreateMonitoringConfig true
            return user.EffectiveCanCreateMonitoringConfig;
        }

        /// <summary>
        /// Cek apakah user bisa update monitoring config (Layer 1 + 2 + 3)
        /// </summary>
        /// <param name="user">User to check permission for</param>
        /// <returns>true jika user bisa update monitoring config, false otherwise</returns>
        public static bool HasUpdateMonitoringConfigPermission(this UserRead user)
        {
            // Layer 1: System/SuperAdmin bypass
            if (user.GroupLevel <= LevelPriority.SuperAdmin)
                return true;

            // Layer 2: Harus PrimaryAdmin
            if (user.GroupLevel != LevelPriority.PrimaryAdmin)
                return false;

            // Layer 2 + 3: Harus IsHead DAN CanUpdateMonitoringConfig true
            return user.EffectiveCanUpdateMonitoringConfig;
        }
    }
}
