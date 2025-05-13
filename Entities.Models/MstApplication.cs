using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models
{
    public class MstApplication
    {

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Generate { get; set; } 

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); 

        
        [Required]
        [StringLength(255)]
        public string ApplicationName { get; set; }
        
        [Required]
        public OrganizationType OrganizationType { get; set; } = OrganizationType.Single;
        
        [Required]
        public string OrganizationAddress { get; set; }
        
        [Required]
        public ApplicationType ApplicationType { get; set; } = ApplicationType.Empty;
        
        [Required]
        public DateTime ApplicationRegistered { get; set; }
        
        [Required]
        public DateTime ApplicationExpired { get; set; }
        
        [Required]
        [StringLength(255)]
        public string HostName { get; set; }
        
        [Required]
        [StringLength(255)]
        public string HostPhone { get; set; }
        
        [Required]
        [StringLength(255)]
        public string HostEmail { get; set; }
        
        [Required]
        [StringLength(255)]
        public string HostAddress { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ApplicationCustomName { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ApplicationCustomDomain { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ApplicationCustomPort { get; set; }
        
        [Required]
        [StringLength(255)]
        public string LicenseCode { get; set; }
        
        [Required]
        public LicenseType LicenseType { get; set; }
        
        [Required]
        public int? ApplicationStatus { get; set; } = 1;

       
       //relasi antar domain table database
       //mstapplication many to ... terhadap table dibawah ini

        public virtual AlarmRecordTracking AlarmRecordTracking { get; set; }
        public virtual ICollection<MstIntegration> Integrations { get; set; } = new List<MstIntegration>();
        public virtual ICollection<MstAccessCctv> AccessCctvs { get; set; } = new List<MstAccessCctv>();
        public virtual ICollection<MstAccessControl> AccessControls { get; set; } = new List<MstAccessControl>();
        public virtual ICollection<MstDepartment> Departments { get; set; } = new List<MstDepartment>();
        public virtual ICollection<MstDistrict> Districts { get; set; } = new List<MstDistrict>();
        public virtual ICollection<MstOrganization> Organizations { get; set; } = new List<MstOrganization>();
        public virtual ICollection<MstMember> Members { get; set; } = new List<MstMember>();
        public virtual ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();        public virtual ICollection<MstBuilding> Buildings { get; set; } = new List<MstBuilding>();
        public virtual ICollection<MstFloorplan> Floorplans { get; set; } = new List<MstFloorplan>();
        public virtual ICollection<FloorplanDevice> FloorplanDevices { get; set; } = new List<FloorplanDevice>();

    
    }
}
