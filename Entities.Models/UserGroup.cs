using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpers.Consumer;

namespace Entities.Models 
{
    // public enum LevelPriority
    // {
    //     System,
    //     SuperAdmin,
    //     PrimaryAdmin,
    //     Primary,
    //     Secondary,
    //     UserCreated
    // }

    public class UserGroup : BaseModel, IApplicationEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string? Name { get; set; }

        [Column("level_priority")]
        public LevelPriority? LevelPriority { get; set; }

        [Required]
        [Column("application_id")]
        public Guid ApplicationId { get; set; }

        [Required]
        [Column("created_by")]
        [StringLength(255)]
        public string CreatedBy { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updated_by")]
        [StringLength(255)]
        public string UpdatedBy { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("status")]
        public int? Status { get; set; } = 1;


        [ForeignKey("ApplicationId")]
        public MstApplication Application { get; set; } // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();

    }
}






