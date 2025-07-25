using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstApplication : BaseModel
    {
        [Required]
        [StringLength(255)]
        [Column("application_name")]
        public string ApplicationName { get; set; }
        
        [Required]
        [Column("organization_type")]
        public OrganizationType OrganizationType { get; set; } = OrganizationType.Single;
        
        [Required]
        [Column("organization_address")]
        public string OrganizationAddress { get; set; }
        
        [Required]
        [Column("application_type")]
        public ApplicationType ApplicationType { get; set; } = ApplicationType.Empty;
        
        [Required]
        [Column("application_registered")]
        public DateTime ApplicationRegistered { get; set; }
        
        [Required]
        [Column("application_expired")]
        public DateTime ApplicationExpired { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("host_name")]
        public string HostName { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("host_phone")]
        public string HostPhone { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("host_email")]
        public string HostEmail { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("host_address")]
        public string HostAddress { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("application_custom_name")]
        public string ApplicationCustomName { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("application_custom_domain")]
        public string ApplicationCustomDomain { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("application_custom_port")]
        public string ApplicationCustomPort { get; set; }
        
        [Required]
        [StringLength(255)]
        [Column("license_code")]
        public string LicenseCode { get; set; }
        
        [Required]
        [Column("license_type")]
        public LicenseType LicenseType { get; set; }
        
        [Required]
        [Column("application_status")]
        public int? ApplicationStatus { get; set; } = 1;

       //relasi antar domain table database
       //mstapplication many to ... terhadap table dibawah ini

        public virtual ICollection<AlarmRecordTracking> AlarmRecordTrackings { get; set; } = new List<AlarmRecordTracking>();
        public virtual ICollection<MstIntegration> Integrations { get; set; } = new List<MstIntegration>();
        public virtual ICollection<MstAccessCctv> AccessCctvs { get; set; } = new List<MstAccessCctv>();
        public virtual ICollection<MstAccessControl> AccessControls { get; set; } = new List<MstAccessControl>();
        public virtual ICollection<MstDepartment> Departments { get; set; } = new List<MstDepartment>();
        public virtual ICollection<MstDistrict> Districts { get; set; } = new List<MstDistrict>();
        public virtual ICollection<MstOrganization> Organizations { get; set; } = new List<MstOrganization>();
        public virtual ICollection<MstMember> Members { get; set; } = new List<MstMember>();
        public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();        
        public virtual ICollection<MstBuilding> Buildings { get; set; } = new List<MstBuilding>();
        public virtual ICollection<MstFloorplan> Floorplans { get; set; } = new List<MstFloorplan>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();
    }
}