using System.Collections.Generic;

namespace Shared.BusinessLogic.Services.Feature
{
    /// <summary>
    /// Feature flag definitions for modules
    /// Core features are always available with base license
    /// Module features require specific module licensing
    /// </summary>
    public static class FeatureDefinition
    {
        // Core Features (included in base license)
        public const string CoreMasterData = "core.masterData";
        public const string CoreTracking = "core.tracking";
        public const string CoreMonitoring = "core.monitoring";
        public const string CoreAlarm = "core.alarm";
        public const string CorePatrol = "core.patrol";
        public const string CoreReporting = "core.reporting";

        // Module Features (add-on features)
        public const string ModuleActiveDirectory = "module.activeDirectory";
        public const string ModuleSso = "module.sso";

        // All core features list
        public static readonly string[] CoreFeatures = new[]
        {
            CoreMasterData,
            CoreTracking,
            CoreMonitoring,
            CoreAlarm,
            CorePatrol,
            CoreReporting
        };

        // All Module features list
        public static readonly string[] ModuleFeatures = new[]
        {
            ModuleActiveDirectory,
            ModuleSso
        };

        // All features
        public static readonly string[] AllFeatures = new[]
        {
            CoreMasterData,
            CoreTracking,
            CoreMonitoring,
            CoreAlarm,
            CorePatrol,
            CoreReporting,
            ModuleActiveDirectory,
            ModuleSso
        };

        // Feature display names
        public static readonly Dictionary<string, string> FeatureDisplayNames = new Dictionary<string, string>
        {
            { CoreMasterData, "Master Data Management" },
            { CoreTracking, "Real-Time Tracking" },
            { CoreMonitoring, "Monitoring Dashboard" },
            { CoreAlarm, "Alarm & Notification" },
            { CorePatrol, "Patrol Management" },
            { CoreReporting, "Reports & Analytics" },
            { ModuleActiveDirectory, "Active Directory Sync" },
            { ModuleSso, "Single Sign-On (SSO)" }
        };

        // Feature descriptions
        public static readonly Dictionary<string, string> FeatureDescriptions = new Dictionary<string, string>
        {
            { CoreMasterData, "Manage master data: employees, visitors, buildings, floors, etc." },
            { CoreTracking, "Real-time BLE tracking and position monitoring" },
            { CoreMonitoring, "Live monitoring dashboard and map views" },
            { CoreAlarm, "Alarm triggers, notifications, and alert management" },
            { CorePatrol, "Patrol route management and checkpoint tracking" },
            { CoreReporting, "Reports, analytics, and data export" },
            { ModuleActiveDirectory, "Automatic employee synchronization with Active Directory" },
            { ModuleSso, "Windows authentication and Single Sign-On integration" }
        };

        // Default features for each license type
        public static readonly Dictionary<string, string[]> DefaultFeatures = new Dictionary<string, string[]>
        {
            {
                "trial",
                new[]
                {
                    CoreMasterData,
                    CoreTracking,
                    CoreMonitoring,
                    CoreAlarm
                }
            },
            {
                "annual",
                new[]
                {
                    CoreMasterData,
                    CoreTracking,
                    CoreMonitoring,
                    CoreAlarm,
                    CorePatrol,
                    CoreReporting
                }
            },
            {
                "perpetual",
                AllFeatures
            }
        };

        /// <summary>
        /// Get display name for a feature key
        /// </summary>
        public static string GetDisplayName(string featureKey)
        {
            return FeatureDisplayNames.GetValueOrDefault(featureKey, featureKey);
        }

        /// <summary>
        /// Get description for a feature key
        /// </summary>
        public static string GetDescription(string featureKey)
        {
            return FeatureDescriptions.GetValueOrDefault(featureKey, "");
        }

        /// <summary>
        /// Check if a feature is a core feature
        /// </summary>
        public static bool IsCoreFeature(string featureKey)
        {
            return ((IList<string>)CoreFeatures).Contains(featureKey);
        }

        /// <summary>
        /// Check if a feature is a module feature
        /// </summary>
        public static bool IsModuleFeature(string featureKey)
        {
            return ((IList<string>)ModuleFeatures).Contains(featureKey);
        }

        /// <summary>
        /// Validate feature key is valid
        /// </summary>
        public static bool IsValidFeature(string featureKey)
        {
            return ((IList<string>)AllFeatures).Contains(featureKey);
        }
    }
}
