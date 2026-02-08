using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    /// <summary>
    /// Security zone mapping for areas
    /// Defines security levels and access rules for different areas
    /// </summary>
    [Table("security_zone_mapping")]
    public class SecurityZoneMapping : BaseModelWithTimeApp
    {
        /// <summary>
        /// Area ID (references FloorplanMaskedArea or similar)
        /// </summary>
        [Required]
        [Column("area_id")]
        public Guid AreaId { get; set; }

        /// <summary>
        /// Area name
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("area_name")]
        public string AreaName { get; set; } = string.Empty;

        /// <summary>
        /// Security zone level: Public=1, Secure=2, Restricted=3, Critical=4
        /// </summary>
        [Required]
        [Column("security_zone")]
        public SecurityZone SecurityZone { get; set; } = SecurityZone.Public;

        /// <summary>
        /// Indicates if this area requires escort for visitors
        /// </summary>
        [Required]
        [Column("requires_escort")]
        public bool RequiresEscort { get; set; }

        /// <summary>
        /// JSON array of zone names from which access is allowed
        /// Example: ["Public", "Secure"] means only from Public or Secure zones
        /// Empty or null means no restrictions
        /// </summary>
        [Column("allowed_from_zones")]
        public string? AllowedFromZones { get; set; }
    }

    /// <summary>
    /// Security zone levels
    /// </summary>
    public enum SecurityZone
    {
        /// <summary>Public area - no access restrictions</summary>
        Public = 1,

        /// <summary>Secure area - requires badge access</summary>
        Secure = 2,

        /// <summary>Restricted area - requires special authorization</summary>
        Restricted = 3,

        /// <summary>Critical area - highest security, requires escort</summary>
        Critical = 4
    }

    /// <summary>
    /// Extension methods for SecurityZone enum
    /// </summary>
    public static class SecurityZoneExtensions
    {
        /// <summary>
        /// Gets the display name for a security zone
        /// </summary>
        public static string GetDisplayName(this SecurityZone zone)
        {
            return zone switch
            {
                SecurityZone.Public => "Public",
                SecurityZone.Secure => "Secure",
                SecurityZone.Restricted => "Restricted",
                SecurityZone.Critical => "Critical",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the risk level for a security zone
        /// </summary>
        public static string GetRiskLevel(this SecurityZone zone)
        {
            return zone switch
            {
                SecurityZone.Public => "Low",
                SecurityZone.Secure => "Low",
                SecurityZone.Restricted => "Medium",
                SecurityZone.Critical => "High",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Checks if transition from one zone to another is allowed
        /// </summary>
        public static bool IsTransitionAllowed(SecurityZone from, SecurityZone to)
        {
            // Public can go anywhere
            if (from == SecurityZone.Public)
                return true;

            // Secure can go to Secure or Public
            if (from == SecurityZone.Secure && to != SecurityZone.Restricted && to != SecurityZone.Critical)
                return true;

            // Restricted can go to Restricted, Secure, or Public
            if (from == SecurityZone.Restricted && to != SecurityZone.Critical)
                return true;

            // Critical can go anywhere
            if (from == SecurityZone.Critical)
                return true;

            return false;
        }
    }
}
